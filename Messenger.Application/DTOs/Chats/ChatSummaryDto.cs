namespace Messenger.Application.DTOs.Chats
{
    public class ChatSummaryDto
    {
        public Guid ChatId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
    }
}
