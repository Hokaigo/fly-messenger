using Messenger.Application.DTOs.Chats;

namespace Messenger.Application.Services.Interfaces
{
    public interface IChatService
    {
        Task<ChatDto> SearchOrCreatePrivateChatAsync(Guid currentUserId, Guid targetUserId);
        Task<ChatDto?> GetChatByIdAsync (Guid chatId);
        Task<List<ChatSummaryDto>> GetChatSummariesAsync(Guid currentUserId);
        Task<ChatSummaryDto?> GetChatSummaryAsync(Guid chatId, Guid userId);

    }
}
