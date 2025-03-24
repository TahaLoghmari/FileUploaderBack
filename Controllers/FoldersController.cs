
using FileUploaderBack.Data;
using FileUploaderBack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using FileUploaderBack.Models.Dto;
[ApiController]
[Route("/api/user/{userId}/[controller]")]
public class FoldersController : ControllerBase
{
    private readonly FileUploaderDbContext _context ; 
    public FoldersController( FileUploaderDbContext context ) 
    {
        _context = context;
    }
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<Folder>> GetFolderById( int userId , int id ) 
    {
        var folder = await _context.Folders.Include(f => f.Files).Include(f => f.ChildFolders)
        .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        if ( folder == null ) return NotFound(); 
        return folder ; 
    }
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<Folder>> GetUserRootFolder ( int userId ) 
    {
        var rootFolder = await _context.Folders
        .Include(f => f.Files).Include(f => f.ChildFolders)
        .Where(f => f.UserId == userId && f.ParentFolderId == null)
        .OrderBy( f => f.Id ).FirstOrDefaultAsync();
        if ( rootFolder == null ) return NotFound() ; 
        return rootFolder ; 
    }
    [HttpPost("addFolder/{id?}")]
    [Authorize]
    public async Task<ActionResult> AddFolder( int userId , int? id , [FromBody] FolderDto dto )
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
        var newFolder = new Folder 
        {
            Name = dto.Name ,
            ParentFolderId = parentFolder.Id ,
            UserId = userId ,
            CreatedAt = DateTime.UtcNow 
        };
        _context.Folders.Add(newFolder);
        await _context.SaveChangesAsync();
        return Ok(newFolder);
    }
    [HttpPut("editFolder/{id}")]
    [Authorize]
    public async Task<ActionResult> EditFolder( int userId , int id , [FromBody] FolderDto dto )
    {
        Folder? folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        
        if ( folder == null)
        {
            return NotFound();
        }
        folder.Name = dto.Name;
        await _context.SaveChangesAsync();
        return Ok(folder);
    }
    [HttpDelete("deleteFolder/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteFolder ( int userId , int id )
    {
        Folder? folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        if ( folder == null ) return NotFound() ; 

        _context.Folders.Remove(folder);
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Folder deleted successfully." });
    }
    
}