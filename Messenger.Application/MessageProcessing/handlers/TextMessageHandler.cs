using Messenger.Application.DTOs.Chats;
using Messenger.Application.MessageProcessing.interfaces;
using Messenger.Application.MessageProcessing.validation;
using Messenger.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.MessageProcessing.handlers
{
    public class TextMessageHandler : MessageHandlerTemplate, ITextMessageHandler
    {
        private readonly IMessageService _msgService;

        public TextMessageHandler(IMessageService msgService) => _msgService = msgService;

        protected override Task ValidateAsyncOrThrow(string? text, IFormFile? file)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidMessageException("Text message cannot be empty.");

            return Task.CompletedTask;
        }


        protected override Task<MessageDto> CreateMessageAsync(Guid chatId, Guid senderId, string? text, IFormFile? file)
            => _msgService.SendMessageAsync(chatId, senderId, text!, null);

        protected override Task SaveMessageAsync(MessageDto message) => Task.CompletedTask;
    }
}
