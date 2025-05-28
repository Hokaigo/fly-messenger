using System;
using System.Linq;
using System.Threading.Tasks;
using Messenger.Application.DTOs.Chats;
using Messenger.Application.Services.Interfaces;
using Messenger.Domain.Repositories;

namespace Messenger.Application.Services.Implementations
{
    public class LastMessageService : ILastMessageService
    {
        private readonly IMessageRepository _messageRepo;

        public LastMessageService(IMessageRepository messageRepo) => _messageRepo = messageRepo;

        public async Task<LastMessageInfoDto?> GetLastMessageInfoAsync(Guid chatId)
        {
            var messages = await _messageRepo.GetMessagesByChatIdAsync(chatId);
            var lastMessage = messages.OrderByDescending(m => m.DateSent).FirstOrDefault();

            if (lastMessage == null)
                return null;

            return new LastMessageInfoDto
            {
                Text = lastMessage.Text,
                DateSent = lastMessage.DateSent
            };
        }
    }
} 