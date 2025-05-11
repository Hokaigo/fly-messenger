using System.Collections.Concurrent;

namespace Messenger.CrossCutting.Services
{
    public class OnlineUserTracker : IOnlineUserTracker
    {
        private readonly ConcurrentDictionary<Guid, int> _online = new ConcurrentDictionary<Guid, int>();

        public void MarkOnline(Guid userId, string connectionId)
        {
            _online.AddOrUpdate(userId, 1, (_, cnt) => cnt + 1);
        }

        public void MarkOffline(Guid userId, string connectionId)
        {
            if (_online.TryGetValue(userId, out var cnt))
            {
                var newCnt = cnt - 1;
                if (newCnt <= 0)
                    _online.TryRemove(userId, out _);
                else
                    _online[userId] = newCnt;
            }
        }

        public bool IsUserOnline(Guid userId)
        {
            return _online.TryGetValue(userId, out var cnt) && cnt > 0;
        }
    }
}
