using FileUploaderBack.Models.Dto;
using FileUploaderBack.Models;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FileUploaderBack.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FileUploaderDbContext _context ; 
    private readonly IConfiguration _configuration;
    public AuthController( FileUploaderDbContext context , IConfiguration configuration) 
    {
        _context = context ; 
        _configuration = configuration;
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLogin user)
    {
        var userFromDb = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == user.Username);
        
        if (userFromDb != null && BCrypt.Net.BCrypt.Verify(user.Password, userFromDb.Password))
        {
            var token = GenerateJwtToken(userFromDb.Username,userFromDb.Id);
            return Ok(new { token });
        }
        
        return Unauthorized();
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserLogin model)
    {
        if (await _context.Users.AnyAsync(u => u.Username == model.Username))
        {
            return BadRequest("Username already exists");
        }
        var user = new User
        {
            Username = model.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        var rootFolder = new Folder
        {
            Name = user.Username, 
            UserId = user.Id,
            ParentFolderId = null, 
            CreatedAt = DateTime.UtcNow
        };
        _context.Folders.Add(rootFolder);
        await _context.SaveChangesAsync();
        var token = GenerateJwtToken(user.Username,user.Id);
        return Ok(new { token });
    }
    private string GenerateJwtToken(string username , int userId)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
    _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured")));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var expiry = DateTime.Now.AddMinutes(
        double.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60"));
        

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: expiry,
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
}