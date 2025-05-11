using Microsoft.AspNetCore.Http;

namespace Messenger.Application.DTOs.Chats
{
    public class SendMessageDto
    {
        public Guid ChatId { get; set; }

        public string? Text { get; set; }

        public IFormFile? File { get; set; }
    }

}
