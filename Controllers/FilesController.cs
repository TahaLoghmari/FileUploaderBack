using FileUploaderBack.Data;
using FileUploaderBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileUploaderBack.Models.Dto;
using FileUploaderBack.Services;

[ApiController]
[Route("/api/user/{userId}/folders/[controller]")]
public class FilesController : ControllerBase
{
    private readonly FileUploaderDbContext _context ; 
    private readonly CloudinaryService _cloudinaryService;
    public FilesController( FileUploaderDbContext context , CloudinaryService cloudinaryService) { _context = context ; _cloudinaryService = cloudinaryService ;}

    [HttpGet("file/{id}")]
    [Authorize]
    public async Task<ActionResult<Filee>> GetFileById(  int id ) 
    {
        var file = await _context.Files.Include( f => f.Folder ).
        FirstOrDefaultAsync(f => f.Id == id );
        if ( file == null ) return NotFound(); 
        return file ; 
    }

    [HttpPost("addFile/{id?}")]
    [Authorize]
    public async Task<ActionResult> AddFile(int userId, int? id, IFormFile file)
    {
        try
        {
            if (file == null)
                return BadRequest("No file uploaded");

            Folder? parentFolder;
            if (id == null) 
                parentFolder = await _context.Folders
                    .Where(f => f.UserId == userId && f.ParentFolderId == null)
                    .FirstOrDefaultAsync();
            else 
                parentFolder = await _context.Folders
                    .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
                    
            if (parentFolder == null)
                return NotFound("Folder not found");

            var uploadResult = await _cloudinaryService.UploadFileAsync(file);
            
            // When you upload a file to Cloudinary, the service returns a URL where that file can be accessed. Without storing this URL, you'd have no way to retrieve the file later.
            var fileEntity = new Filee
            {
                Name = file.FileName,
                Size = file.Length,
                FolderId = parentFolder.Id,
                UploadPath = uploadResult.SecureUrl.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Files.Add(fileEntity);
            await _context.SaveChangesAsync();
            
            return Ok(fileEntity);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [HttpDelete("deleteFile/{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteFile( int id  )
    {
        Filee? file = await _context.Files.FirstOrDefaultAsync( f => f.Id == id );
        if ( file == null)
        {
            return NotFound();
        }
        if (!string.IsNullOrEmpty(file.UploadPath))
        {
            try
            {
                var uri = new Uri(file.UploadPath);
                var pathSegments = uri.AbsolutePath.Split('/');
                var filename = pathSegments[pathSegments.Length - 1];
                var publicId = filename.Substring(0, filename.LastIndexOf('.'));
                await _cloudinaryService.DeleteFileAsync(publicId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting from Cloudinary: {ex.Message}");
            }
        }
        _context.Files.Remove(file);
        await _context.SaveChangesAsync();
        return Ok(new { message = "File Deleted Successfully !"});
    }
    [HttpGet("download/{id}")]
    [Authorize]
    public async Task<IActionResult> DownloadFile(int id)
    {
        Filee? file = await _context.Files.FirstOrDefaultAsync(f => f.Id == id);
        if (file == null)
            return NotFound();

        string downloadUrl = file.UploadPath + "?fl_attachment=true&resource_type=raw";

        try
        {
            using var httpClient = new HttpClient();
  
            using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Error downloading file from Cloudinary");

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            Console.WriteLine($"Downloaded file length: {fileBytes.Length} bytes");
            
            return File(fileBytes, contentType, file.Name);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
    [HttpPost("share/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> ShareFile(int id, [FromQuery] string duration)
    {
        var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == id);
        if (file == null)
            return NotFound("File not found.");

        // Validate duration: must be something like "1d" or "10d"
        if (string.IsNullOrWhiteSpace(duration) || !duration.EndsWith("d") || !int.TryParse(duration.TrimEnd('d'), out int days))
        {
            return BadRequest("Duration must be specified in the format 'Xd' (e.g. '1d', '10d').");
        }

        // Generate a share token and expiry date.
        Guid shareToken = Guid.NewGuid();
        DateTime expiry = DateTime.UtcNow.AddDays(days);

        // For demonstration we simply return the share URL.
        // In production, store (shareToken, file.Id, expiry) in a dedicated table for later validation.
        string shareLink = $"http://localhost:5173/FileUploader/share/{shareToken}";

        return Ok(new { shareLink, expiry });
    }
}