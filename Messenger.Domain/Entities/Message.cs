namespace Messenger.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ChatId { get; set; }
        public Guid UserId {  get; set; }

        public string Text { get; set; } = string.Empty;
        public DateTime DateSent { get; set; } = DateTime.UtcNow;

        public Chat Chat { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
