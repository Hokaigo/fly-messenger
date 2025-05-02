using Messenger.Application.DTOs.Chats;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.Services.Interfaces
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid chatId);
        Task<MessageDto> SendMessageAsync(Guid chatId, Guid userId, string? text, IFormFile? file);
        Task<MessageDto> EditMessageAsync(Guid messageId, string newText);
        Task<Guid> DeleteMessageAsync(Guid messageId);
    }
}
