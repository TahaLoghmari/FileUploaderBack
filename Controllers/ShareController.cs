using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FileUploaderBack.Data;
using FileUploaderBack.Models;

[ApiController]
[Route("api/[controller]")]
public class ShareController : ControllerBase
{
    private readonly FileUploaderDbContext _context;

    public ShareController(FileUploaderDbContext context)
    {
        _context = context;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> GetSharedFile(string token)
    {
        var sharedFile = await _context.SharedFiles
            .Include(sf => sf.File)
            .FirstOrDefaultAsync(sf => sf.Token == token);

        if (sharedFile == null)
            return NotFound("Shared file not found");

        if (sharedFile.ExpiresAt < DateTime.UtcNow)
            return BadRequest("Share link has expired");

        var file = sharedFile.File;

        return Ok(new {
            id = file.Id,
            name = file.Name,
            uploadPath = file.UploadPath,
            size = file.Size,
            contentType = GetContentType(file.Name)
        });
    }

    private string GetContentType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" or ".docx" => "application/msword",
            ".xls" or ".xlsx" => "application/vnd.ms-excel",
            ".ppt" or ".pptx" => "application/vnd.ms-powerpoint",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}