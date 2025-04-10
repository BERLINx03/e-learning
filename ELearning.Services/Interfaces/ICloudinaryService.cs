using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using ELearning.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ELearning.Services
{
    public interface ICloudinaryService
    {
        Task<BaseResult<string>> UploadImageAsync(IFormFile file);
        Task<BaseResult<bool>> DeleteImageAsync(string publicId);
        Task<BaseResult<string>> UploadVideoAsync(IFormFile file);
        Task<BaseResult<bool>> DeleteVideoAsync(string publicId);
    }
}