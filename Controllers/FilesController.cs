
using FileUploaderBack.Data;
using FileUploaderBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using FileUploaderBack.Models.Dto;

[ApiController]
[Route("/api/user/{userId}/folders/[controller]")]
public class FilesController : ControllerBase
{
    private readonly FileUploaderDbContext _context ; 
    public FilesController( FileUploaderDbContext context ) { _context = context ; }

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
    public async Task<ActionResult> AddFile( int userId , int? id  , [FromBody] FileDto dto)
    {
        Folder? parentFolder;
        if ( id == null) parentFolder = await _context.Folders
                .Where(f => f.UserId == userId && f.ParentFolderId == null)
                .OrderBy(f => f.Id)
                .FirstOrDefaultAsync();
        
        else parentFolder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        
        if ( parentFolder == null)
        {
            return NotFound();
        }
        var file = new Filee
        {
            Name = dto.Name,
            Size = dto.Size,
            FolderId = parentFolder.Id,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Files.Add(file);
        await _context.SaveChangesAsync();
        
        return Ok(file);
    }
    [HttpPut("editFile/{id}")]
    [Authorize]
    public async Task<ActionResult> EditFile( int id , [FromBody] FileDto dto )
    {
        Filee? file = await _context.Files.FirstOrDefaultAsync( f => f.Id == id );
        if ( file == null)
        {
            return NotFound();
        }
        file.Name = dto.Name;
        await _context.SaveChangesAsync();
        return Ok(file);
    }
    [HttpDelete("deleteFile/{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteFile( int id , [FromBody] FileDto dto )
    {
        Filee? file = await _context.Files.FirstOrDefaultAsync( f => f.Id == id );
        if ( file == null)
        {
            return NotFound();
        }
        _context.Files.Remove(file);
        await _context.SaveChangesAsync();
        return Ok(new { message = "File Deleted Successfully !"});
    }
}