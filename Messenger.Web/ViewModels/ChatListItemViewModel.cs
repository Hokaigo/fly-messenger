namespace Messenger.Web.ViewModels
{
    public class ChatListItemViewModel
    {
        public Guid ChatId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
    }
}   