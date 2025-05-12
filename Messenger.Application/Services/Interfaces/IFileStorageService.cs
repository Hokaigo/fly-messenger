namespace Messenger.Application.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(Stream stream, string fileName, string subfolder);
        Task DeleteAsync(string relativePath);
    }
}
