using Messenger.Application.DTOs.Chats;
using Microsoft.AspNetCore.Http;

namespace Messenger.Application.MessageProcessing.interfaces
{
    public interface IMessageHandler
    {
        IMessageHandler SetNext(IMessageHandler next);
        Task<MessageDto> ProcessAsync(Guid chatId, Guid senderId, string? text, IFormFile? file);
    }
}
