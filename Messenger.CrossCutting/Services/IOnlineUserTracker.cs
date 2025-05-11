using System;

namespace Messenger.CrossCutting.Services
{
    public interface IOnlineUserTracker
    {
        void MarkOnline(Guid userId, string connectionId);
        void MarkOffline(Guid userId, string connectionId);
        bool IsUserOnline(Guid userId);
    }
}
