using AutoMapper;
using Messenger.Application.DTOs.Chats;
using Messenger.Application.Services.Factories;
using Messenger.Application.Services.Interfaces;
using Messenger.Domain.Entities;
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
        private readonly ILastMessageService _lastMessageService;

        public ChatService(IChatRepository chatRepo,IUserRepository userRepo,IMessageRepository messageRepo,IMapper mapper,IChatFactory chatFactory,
            ILastMessageService lastMessageService)
        {
            _chatRepo = chatRepo;
            _userRepo = userRepo;
            _messageRepo = messageRepo;
            _mapper = mapper;
            _chatFactory = chatFactory;
            _lastMessageService = lastMessageService;
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
            var summaries = new List<ChatSummaryDto>();

            foreach (var chat in chats)
            {
                var summary = await CreateChatSummaryAsync(chat, currentUserId);
                if (summary != null)
                    summaries.Add(summary);
            }

            return summaries.OrderByDescending(d => d.LastMessageTime ?? DateTime.MinValue).ToList();
        }

        public async Task<ChatSummaryDto?> GetChatSummaryAsync(Guid chatId, Guid userId)
        {
            var chat = await _chatRepo.GetByIdAsync(chatId);
            if (chat == null || !chat.Members.Any(m => m.Id == userId))
                return null;

            return await CreateChatSummaryAsync(chat, userId);
        }

        private async Task<ChatSummaryDto?> CreateChatSummaryAsync(Chat chat, Guid currentUserId)
        {
            var otherUser = chat.Members.FirstOrDefault(m => m.Id != currentUserId);
            if (otherUser == null)
                return null;

            var dto = _mapper.Map<ChatSummaryDto>(chat);
            dto.OtherUserName = otherUser.UserName;

            var lastMessageInfo = await _lastMessageService.GetLastMessageInfoAsync(chat.Id);
            if (lastMessageInfo != null)
            {
                dto.LastMessage = lastMessageInfo.Text;
                dto.LastMessageTime = lastMessageInfo.DateSent;
            }

            return dto;
        }
    }
}
