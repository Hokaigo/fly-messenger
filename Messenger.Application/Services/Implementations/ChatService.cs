using AutoMapper;
using Messenger.Application.DTOs.Chats;
using Messenger.Application.Services.Factories;
using Messenger.Application.Services.Interfaces;
using Messenger.Domain.Repositories;

namespace Messenger.Application.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMessageRepository _messageRepo;
        private readonly IMapper _mapper;
        private readonly IChatFactory _chatFactory;

        public ChatService(IChatRepository chatRepo, IUserRepository userRepo, IMessageRepository messageRepo, IMapper mapper,
            IChatFactory chatFactory)
        {
            _chatRepo = chatRepo;
            _userRepo = userRepo;
            _messageRepo = messageRepo;
            _mapper = mapper;
            _chatFactory = chatFactory;
        }

        public async Task<ChatDto> SearchOrCreatePrivateChatAsync(Guid currentUserId, Guid targetUserId)
        {
            var existing = await _chatRepo.GetPrivateChatBetweenUsersAsync(currentUserId, targetUserId);
            if (existing != null)
            {
                var full = await _chatRepo.GetByIdAsync(existing.Id);
                return _mapper.Map<ChatDto>(full);
            }

            var thisUser = await _userRepo.GetByIdAsync(currentUserId) ?? throw new InvalidOperationException("Current user not found");
            var otherUser = await _userRepo.GetByIdAsync(targetUserId) ?? throw new InvalidOperationException("Target user not found");

            var chat = _chatFactory.CreatePrivateChat(thisUser, otherUser);

            await _chatRepo.AddAsync(chat);
            return _mapper.Map<ChatDto>(chat);
        }

        public async Task<ChatDto?> GetChatByIdAsync(Guid chatId)
        {
            var chat = await _chatRepo.GetByIdAsync(chatId);
            if (chat == null) return null;

            var msgs = await _messageRepo.GetMessagesByChatIdAsync(chatId);
            chat.Messages = msgs.ToList();
            return _mapper.Map<ChatDto>(chat);
        }

        public async Task<List<ChatSummaryDto>> GetChatSummariesAsync(Guid currentUserId)
        {
            var chats = await _chatRepo.GetUserChatsAsync(currentUserId);
            var dtos = new List<ChatSummaryDto>();

            foreach (var chat in chats)
            {
                var otherUser = chat.Members.FirstOrDefault(m => m.Id != currentUserId);
                if (otherUser == null)
                    continue;

                var dto = new ChatSummaryDto
                {
                    ChatId = chat.Id,
                    OtherUserName = otherUser.UserName
                };

                var messages = await _messageRepo.GetMessagesByChatIdAsync(chat.Id);
                var lastMessage = messages.OrderByDescending(m => m.DateSent).FirstOrDefault();

                if (lastMessage != null)
                {
                    dto.LastMessage = lastMessage.Text;
                    dto.LastMessageTime = lastMessage.DateSent;
                }

                dtos.Add(dto);
            }

            return dtos.OrderByDescending(d => d.LastMessageTime ?? DateTime.MinValue).ToList();
        }

        public async Task<ChatSummaryDto?> GetChatSummaryAsync(Guid chatId, Guid userId)
        {
            var chat = await _chatRepo.GetByIdAsync(chatId);
            if (chat == null || !chat.Members.Any(m => m.Id == userId))
                return null;

            var dto = _mapper.Map<ChatSummaryDto>(chat);
            var other = chat.Members.First(m => m.Id != userId);
            dto.OtherUserName = other.UserName;

            var last = (await _messageRepo.GetMessagesByChatIdAsync(chatId))
                .OrderByDescending(m => m.DateSent)
                .FirstOrDefault();

            if (last != null)
            {
                dto.LastMessage = last.Text;
                dto.LastMessageTime = last.DateSent;
            }

            return dto;
        }

    }
}
