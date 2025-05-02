using Messenger.Domain.Enums;

namespace Messenger.Application.DTOs.Chats
{
    public class ChatDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ChatType Type { get; set; }
        public List<UserDto> Members { get; set; } = new List<UserDto>();
        public List<MessageDto> Messages { get; set; } = new List<MessageDto>();
    }
}
