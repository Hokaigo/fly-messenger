using Messenger.Application.DTOs.Chats;
using Messenger.Application.MessageProcessing.interfaces;
using Messenger.Application.MessageProcessing.validation;
using Messenger.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.MessageProcessing.handlers
{
    public class FileMessageHandler : MessageHandlerTemplate, IFileMessageHandler
    {
        private readonly IMessageService _msgService;
        private static readonly string[] AllowedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".docx", ".mp4", ".mp3" };
        private const long MaxFileSize = 15 * 1024 * 1024;

        public FileMessageHandler(IMessageService msgService) => _msgService = msgService;

        protected override Task ValidateAsyncOrThrow(string? text, IFormFile? file)
        {
            if (file == null)
                throw new InvalidMessageException("File is required.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                var allowed = string.Join(", ", AllowedExtensions);
                throw new InvalidMessageException($"File extension \"{ext}\" is not supported (allowed: {allowed}).");
            }

            if (file.Length > MaxFileSize)
                throw new InvalidMessageException($"File too large (maximum {MaxFileSize / (1024 * 1024)} MB).");

            return Task.CompletedTask;
        }

        protected override Task<MessageDto> CreateMessageAsync(Guid chatId, Guid senderId, string? text, IFormFile? file)
            => _msgService.SendMessageAsync(chatId, senderId, text, file);

        protected override Task SaveMessageAsync(MessageDto message) => Task.CompletedTask;
    }
}
