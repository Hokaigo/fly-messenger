using Messenger.Domain.Enums;

namespace Messenger.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ChatId { get; set; }
        public Guid UserId {  get; set; }

        public MessageType Type { get; set; } = MessageType.Text;

        public string Text { get; set; } = string.Empty;

        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
        public long? FileSize { get; set; }

        public DateTime DateSent { get; set; } = DateTime.UtcNow;

        public Chat Chat { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
