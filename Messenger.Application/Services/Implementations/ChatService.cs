using Messenger.Application.DTOs.Chats;
using Messenger.Domain.Entities;
using Messenger.Domain.Repositories;
using Messenger.Domain.Enums;
using Messenger.Application.Services.Interfaces;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepo;
    private readonly IUserRepository _userRepo;
    private readonly IMessageRepository _messageRepo;

    public ChatService(IChatRepository chatRepo, IUserRepository userRepo, IMessageRepository messageRepo)
    {
        _chatRepo = chatRepo;
        _userRepo = userRepo;
        _messageRepo = messageRepo;
    }

    public async Task<ChatDto> SearchOrCreatePrivateChatAsync(Guid currentUserId, Guid targetUserId)
    {
        var existing = await _chatRepo.GetPrivateChatBetweenUsersAsync(currentUserId, targetUserId);
        if (existing != null)
        {
            var existingChat = await _chatRepo.GetByIdAsync(existing.Id);
            return MapToChatDto(existingChat);
        }

        var thisUser = await _userRepo.GetByIdAsync(currentUserId) ?? throw new InvalidOperationException("Current user not found");
        var otherUser = await _userRepo.GetByIdAsync(targetUserId) ?? throw new InvalidOperationException("Target user not found");

        var chat = new Chat
        {
            Name = $"{thisUser.UserName}/{otherUser.UserName}",
            Type = ChatType.Private,
            Members = new List<User> { thisUser, otherUser }
        };

        await _chatRepo.AddAsync(chat);

        return MapToChatDto(chat);
    }

    public async Task<ChatDto?> GetChatByIdAsync(Guid chatId)
    {
        var chat = await _chatRepo.GetByIdAsync(chatId);
        if (chat == null) return null;

        var msgs = await _messageRepo.GetMessagesByChatIdAsync(chatId);
        chat.Messages = msgs.ToList();

        return MapToChatDto(chat);
    }

    private ChatDto MapToChatDto(Chat c)
    {
        return new ChatDto
        {
            Id = c.Id,
            Name = c.Name,
            Type = c.Type,
            Members = c.Members
                         .Select(u => new UserDto { Id = u.Id, UserName = u.UserName, Email = u.Email })
                         .ToList(),
            Messages = c.Messages.OrderBy(m => m.DateSent)
                         .Select(m => new MessageDto
                         {
                             Id = m.Id,
                             ChatId = m.ChatId,
                             UserId = m.UserId,
                             UserName = m.User.UserName,
                             DateSent = m.DateSent,
                             Type = m.Type,
                             Text = m.Text,
                             FileUrl = m.FileUrl,
                             FileName = m.FileName,
                             FileType = m.FileType,
                             FileSize = m.FileSize
                         }).ToList()
        };
    }


    public async Task<List<ChatSummaryDto>> GetChatSummariesAsync(Guid currentUserId)
    {
        var chats = await _chatRepo.GetUserChatsAsync(currentUserId);
        var summaries = new List<ChatSummaryDto>();

        foreach (var chat in chats)
        {
            var other = chat.Members.First(m => m.Id != currentUserId);
            var otherUserName = other.UserName;

            var allMsgs = (await _messageRepo.GetMessagesByChatIdAsync(chat.Id)).OrderBy(m => m.DateSent);
            var lastMsg = allMsgs.LastOrDefault();

            summaries.Add(new ChatSummaryDto
            {
                ChatId = chat.Id,
                OtherUserName = otherUserName,
                LastMessage = lastMsg?.Text,
                LastMessageTime = lastMsg?.DateSent
            });
        }

        return summaries.OrderByDescending(x => x.LastMessageTime ?? DateTime.MinValue).ToList();
    }


    public async Task<ChatSummaryDto?> GetChatSummaryAsync(Guid chatId, Guid userId)
    {
        var chat = await _chatRepo.GetByIdAsync(chatId);
        if (chat == null || !chat.Members.Any(m => m.Id == userId))
            return null;

        var other = chat.Members.FirstOrDefault(m => m.Id != userId);
        var otherUserName = other?.UserName ?? "Unknown User";

        var lastMsg = (await _messageRepo.GetMessagesByChatIdAsync(chatId)).OrderByDescending(m => m.DateSent).FirstOrDefault();

        return new ChatSummaryDto
        {
            ChatId = chat.Id,
            OtherUserName = otherUserName,
            LastMessage = lastMsg?.Text,
            LastMessageTime = lastMsg?.DateSent
        };
    }
}
