namespace FileUploaderBack.Models.Dto
{
    public class FolderDto 
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public string UploadPath { get; set; } = string.Empty;
    }
}
