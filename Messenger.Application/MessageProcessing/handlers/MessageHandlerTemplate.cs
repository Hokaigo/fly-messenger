using Messenger.Application.DTOs.Chats;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.MessageProcessing.handlers
{
    public abstract class MessageHandlerTemplate
    {
        public async Task<MessageDto> ProcessAsync(Guid chatId, Guid senderId, string? text, IFormFile? file)
        {
            await ValidateAsyncOrThrow(text, file);
            var message = await CreateMessageAsync(chatId, senderId, text, file);
            await SaveMessageAsync(message);
            return message;
        }

        protected abstract Task ValidateAsyncOrThrow(string? text, IFormFile? file);
        protected abstract Task<MessageDto> CreateMessageAsync(Guid chatId, Guid senderId, string? text, IFormFile? file);
        protected abstract Task SaveMessageAsync(MessageDto message);
    }

}
