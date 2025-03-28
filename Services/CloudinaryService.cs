using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace FileUploaderBack.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        
        public CloudinaryService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );
            
            _cloudinary = new Cloudinary(account);
        }
        public async Task<DeletionResult> DeleteFileAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
        public async Task<UploadResult> UploadFileAsync(IFormFile file)
        {
            if (file.Length <= 0)
                throw new ArgumentException("File is empty");
                
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0; 

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension == ".pdf") 
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, ms),
                    UseFilename = true,
                    UniqueFilename = true,
                    Folder = "file-uploader"
                };
                
                Console.WriteLine("Uploading PDF as raw file type");
                return await _cloudinary.UploadAsync(uploadParams);
            }
            else if (IsImageFile(extension))
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, ms),
                    UseFilename = true,
                    UniqueFilename = true,
                    Folder = "file-uploader"
                };
                
                return await _cloudinary.UploadAsync(uploadParams);
            }
            else 
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, ms),
                    UseFilename = true,
                    UniqueFilename = true,
                    Folder = "file-uploader"
                };
                
                return await _cloudinary.UploadAsync(uploadParams);
            }
        }

        private bool IsImageFile(string extension)
        {
            return extension == ".jpg" || extension == ".jpeg" || 
                extension == ".png" || extension == ".gif" || 
                extension == ".bmp" || extension == ".webp";
        }
    }
}