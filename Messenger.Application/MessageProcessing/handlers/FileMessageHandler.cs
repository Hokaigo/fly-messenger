using Messenger.Application.DTOs.Chats;
using Messenger.Application.MessageProcessing.interfaces;
using Messenger.Application.MessageProcessing.validation;
using Messenger.Application.Services.Interfaces;
using Messenger.Domain.Enums;
using Messenger.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.MessageProcessing.handlers
{
    public class FileMessageHandler : MessageHandlerTemplate, IFileMessageHandler
    {
        private readonly IFileStorageService _storage;
        private readonly IUserRepository _userRepo;

        private static readonly string[] AllowedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".docx", ".mp4", ".mp3" };
        private const long MaxFileSize = 15 * 1024 * 1024;

        public FileMessageHandler(IFileStorageService storage, IUserRepository userRepo)
        {
            _storage = storage;
            _userRepo = userRepo;
        }

        protected override async Task ValidateAsync(string? text, IFormFile? file)
        {
            if (file == null)
                throw new InvalidMessageException("File is required.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidMessageException($"File extension \"{ext}\" is not supported.");

            if (file.Length > MaxFileSize)
                throw new InvalidMessageException($"File too large (max {MaxFileSize / (1024 * 1024)} MB).");
        }

        public override async Task<MessageDto> ProcessAsync(Guid chatId, Guid senderId, string? text, IFormFile? file)
        {
            if (file == null)
                return await _next!.ProcessAsync(chatId, senderId, text, file);

            return await base.ProcessAsync(chatId, senderId, text, file);
        }

        protected override async Task<MessageDto> CreateMessageAsync(Guid chatId, Guid senderId, string? text, IFormFile? file)
        {
            string? fileUrl = null;

            if (file != null)
            {
                await using var stream = file.OpenReadStream();
                fileUrl = await _storage.SaveAsync(stream, file.FileName, "uploads");
            }

            var user = await _userRepo.GetByIdAsync(senderId);
            var userName = user?.UserName ?? "Unknown"; 

            var dto = new MessageDto
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                UserId = senderId,
                UserName = userName,
                DateSent = DateTime.UtcNow,
                Type = file == null ? MessageType.Text : MessageType.File,
                Text = text?.Trim() ?? string.Empty,
                FileUrl = fileUrl, 
                FileName = file?.FileName,
                FileType = file?.ContentType,
                FileSize = file?.Length
            };

            return dto;
        }
    }
}
