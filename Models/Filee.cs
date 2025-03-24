namespace FileUploaderBack.Models
{
    public class Filee 
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; } 
    public string UploadPath { get; set; } = string.Empty;
    public int FolderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Folder Folder { get; set; } = null!;
}
}