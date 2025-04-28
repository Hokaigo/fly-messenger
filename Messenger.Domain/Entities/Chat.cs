using Messenger.Domain.Enums;

namespace Messenger.Domain.Entities
{
    public class Chat
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public ChatType Type {  get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<User> Members { get; set; } = new List<User>(); 
    }
}
