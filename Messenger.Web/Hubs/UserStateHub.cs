using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Messenger.CrossCutting.Services;

namespace Messenger.Web.Hubs
{
    public class UserStateHub : Hub
    {
        private readonly IOnlineUserTracker _tracker;

        public UserStateHub(IOnlineUserTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var strId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(strId, out var userId))
            {
                _tracker.MarkOnline(userId, Context.ConnectionId);
                await Clients.All.SendAsync("UserStatusChanged", userId, true);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var strId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(strId, out var userId))
            {
                _tracker.MarkOffline(userId, Context.ConnectionId);
                if (!_tracker.IsUserOnline(userId))
                    await Clients.All.SendAsync("UserStatusChanged", userId, false);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
