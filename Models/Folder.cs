namespace FileUploaderBack.Models
{
    public class Folder 
{
    public int Id { get; set; } 
    public string Name { get; set; } = string.Empty;
    public int? ParentFolderId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public Folder? ParentFolder { get; set; }
    // these collections are empty by default , you need to load them using Include
    public ICollection<Folder> ChildFolders { get; set; } = new List<Folder>();
    public ICollection<Filee> Files { get; set; } = new List<Filee>();
}
}