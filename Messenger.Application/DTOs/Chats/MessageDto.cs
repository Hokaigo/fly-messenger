using Messenger.Domain.Enums;

namespace Messenger.Application.DTOs.Chats
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime DateSent { get; set; }

        public MessageType Type { get; set; }

        public string Text { get; set; }

        public  string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public long? FileSize { get; set; }
    }
}
