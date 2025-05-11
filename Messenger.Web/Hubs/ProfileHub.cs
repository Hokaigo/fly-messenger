using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.RegularExpressions;

public class ProfileHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var me = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (me != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, me);
        await base.OnConnectedAsync();
    }

    public Task JoinProfileGroup(string profileUserId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, profileUserId);
    }
}
