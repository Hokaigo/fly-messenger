using Messenger.Application.DTOs.Chats;
using Messenger.Application.MessageProcessing.interfaces;
using Messenger.Application.MessageProcessing.validation;
using Microsoft.AspNetCore.Http;
using Messenger.Domain.Enums;
using Messenger.Domain.Repositories;

namespace Messenger.Application.MessageProcessing.handlers
{
    public class TextMessageHandler : MessageHandlerTemplate, ITextMessageHandler
    {
        private readonly IEnumerable<IMessageValidator> _validators;
        private readonly IUserRepository _userRepo;

        public TextMessageHandler(IEnumerable<IMessageValidator> validators, IUserRepository userRepo)
        {
            _validators = validators;
            _userRepo = userRepo;
        }

        protected override async Task ValidateAsync(string? text, IFormFile? file)
        {
            foreach (var v in _validators)
                await v.ValidateAsync(text);

            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidMessageException("Text message cannot be empty.");
        }

        protected override async Task<MessageDto> CreateMessageAsync(Guid chatId, Guid senderId, string? text, IFormFile? file)
        {
            var user = await _userRepo.GetByIdAsync(senderId);
            var userName = user?.UserName ?? "Unknown"; 

            var dto = new MessageDto
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                UserId = senderId,
                UserName = userName,
                DateSent = DateTime.UtcNow,
                Type = MessageType.Text,
                Text = text!.Trim()
            };

            return dto;
        }

    }
}
