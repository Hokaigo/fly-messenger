using Messenger.CrossCutting.Services;
using System.Collections.Concurrent;

public class OnlineUserTracker : IOnlineUserTracker
{
    private readonly ConcurrentDictionary<Guid, int> _userConnectionCount = new();

    public void MarkOnline(Guid userId, string connectionId)
    {
        _userConnectionCount.AddOrUpdate(userId, 1, (_, current) => current + 1);
    }

    public void MarkOffline(Guid userId, string connectionId)
    {
        if (_userConnectionCount.TryGetValue(userId, out var count))
        {
            if (count <= 1)
                _userConnectionCount.TryRemove(userId, out _);
            else
                _userConnectionCount[userId] = count - 1;
        }
    }

    public bool IsUserOnline(Guid userId) => _userConnectionCount.TryGetValue(userId, out var count) && count > 0;
}
