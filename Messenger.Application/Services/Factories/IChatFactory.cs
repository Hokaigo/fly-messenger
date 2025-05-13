using Messenger.Domain.Entities;

namespace Messenger.Application.Services.Factories
{
    public interface IChatFactory
    {
        Chat CreatePrivateChat(User thisUser, User otherUser);
    }
}
