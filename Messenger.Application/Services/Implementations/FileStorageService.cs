using Messenger.Application.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Messenger.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        public FileStorageService(IWebHostEnvironment env) => _env = env;

        public async Task<string> SaveAsync(Stream stream, string fileName, string subfolder)
        {
            var webRoot = _env.WebRootPath;
            var folderPath = Path.Combine(webRoot, subfolder);
            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);
            await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);

            var relative = Path.Combine(subfolder, fileName).Replace(Path.DirectorySeparatorChar, '/');

            return "/" + relative;   
        }


        public Task DeleteAsync(string relativePath)
        {
            var webRoot = _env.WebRootPath;
            var fullPath = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
                File.Delete(fullPath);

            return Task.CompletedTask;
        }
    }
}
