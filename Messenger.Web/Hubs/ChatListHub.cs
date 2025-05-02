using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Messenger.Web.Hubs
{
    public class ChatListHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userIdClaim.Value);
            }
            await base.OnConnectedAsync();
        }
    }

}
