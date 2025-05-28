using Messenger.Application.DTOs.Chats;

namespace Messenger.Application.Services.Interfaces
{
    public interface ILastMessageService
    {
        Task<LastMessageInfoDto?> GetLastMessageInfoAsync(Guid chatId);
    }
}
