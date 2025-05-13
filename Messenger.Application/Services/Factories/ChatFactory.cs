using Messenger.Domain.Entities;
using Messenger.Domain.Enums;

namespace Messenger.Application.Services.Factories
{
    public class ChatFactory : IChatFactory
    {
        public Chat CreatePrivateChat(User thisUser, User otherUser)
        {
            return new Chat
            {
                Name = $"{thisUser.UserName}/{otherUser.UserName}",
                Type = ChatType.Private,
                Members = new List<User> { thisUser, otherUser }
            };
        }
    }
}
