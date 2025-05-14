using Messenger.Application.DTOs.Chats;
using Messenger.Application.MessageProcessing.interfaces;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.MessageProcessing.handlers
{
    public abstract class MessageHandlerTemplate : IMessageHandler
    {
        protected IMessageHandler? _next;

        public IMessageHandler SetNext(IMessageHandler next)
        {
            _next = next;
            return next;
        }

        public virtual async Task<MessageDto> ProcessAsync(Guid chatId, Guid senderId, string? text, IFormFile? file)
        {
            await ValidateAsync(text, file);

            var dto = await CreateMessageAsync(chatId, senderId, text, file);
            await SaveAsync(dto);

            return dto;
        }


        protected abstract Task ValidateAsync(string? text, IFormFile? file);
        protected abstract Task<MessageDto> CreateMessageAsync(
            Guid chatId, Guid senderId, string? text, IFormFile? file);
        protected virtual Task SaveAsync(MessageDto dto) => Task.CompletedTask;
    }
}
