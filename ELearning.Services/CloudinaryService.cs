using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ELearning.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ELearning.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<BaseResult<string>> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BaseResult<string>.Fail(["No file was uploaded"]);
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BaseResult<string>.Fail(["Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed."]);
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BaseResult<string>.Fail(["File size exceeds the maximum limit of 5MB"]);
                }

                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill"),
                    Folder = "e-learning/profiles"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                {
                    return BaseResult<string>.Fail([$"Upload failed: {uploadResult.Error.Message}"]);
                }

                return BaseResult<string>.Success(uploadResult.SecureUrl.ToString(), "Image uploaded successfully");
            }
            catch (Exception ex)
            {
                return BaseResult<string>.Fail([$"Upload failed: {ex.Message}"]);
            }
        }

        public async Task<BaseResult<bool>> DeleteImageAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                {
                    return BaseResult<bool>.Fail(["Public ID cannot be empty"]);
                }

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Error != null)
                {
                    return BaseResult<bool>.Fail([$"Deletion failed: {result.Error.Message}"]);
                }

                return BaseResult<bool>.Success(true, "Image deleted successfully");
            }
            catch (Exception ex)
            {
                return BaseResult<bool>.Fail([$"Deletion failed: {ex.Message}"]);
            }
        }

        public async Task<BaseResult<string>> UploadVideoAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BaseResult<string>.Fail(["No file was uploaded"]);
                }

                // Validate file type
                var allowedExtensions = new[] { ".mp4", ".mov", ".avi", ".wmv" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BaseResult<string>.Fail(["Invalid file type. Only MP4, MOV, AVI, and WMV are allowed."]);
                }

                // Validate file size (max 500MB)
                if (file.Length > 500 * 1024 * 1024)
                {
                    return BaseResult<string>.Fail(["File size exceeds the maximum limit of 500MB"]);
                }

                using var stream = file.OpenReadStream();
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "e-learning/videos",
                    // ResourceType = ResourceType.Video,
                    Transformation = new Transformation().Width(1280).Height(720).Crop("fill")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.Error != null)
                {
                    return BaseResult<string>.Fail([$"Upload failed: {uploadResult.Error.Message}"]);
                }

                return BaseResult<string>.Success(uploadResult.SecureUrl.ToString(), "Video uploaded successfully");
            }
            catch (Exception ex)
            {
                return BaseResult<string>.Fail([$"Upload failed: {ex.Message}"]);
            }
        }

        public async Task<BaseResult<bool>> DeleteVideoAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                {
                    return BaseResult<bool>.Fail(["Public ID cannot be empty"]);
                }

                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Video
                };

                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.Error != null)
                {
                    return BaseResult<bool>.Fail([$"Deletion failed: {result.Error.Message}"]);
                }

                return BaseResult<bool>.Success(true, "Video deleted successfully");
            }
            catch (Exception ex)
            {
                return BaseResult<bool>.Fail([$"Deletion failed: {ex.Message}"]);
            }
        }
    }
}