using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            var sessionId = Context.GetHttpContext().Request.Query["sessionId"].ToString();
            if (!string.IsNullOrEmpty(sessionId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
                // Optionally, send a confirmation message to the client
            }
        }
        catch (Exception ex)
        {
            // Handle or log the error
            Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            var sessionId = Context.GetHttpContext().Request.Query["sessionId"].ToString();
            if (!string.IsNullOrEmpty(sessionId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
            }
        }
        catch (Exception ex)
        {
            // Handle or log the error
            Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendForceLogout(string sessionId, string message)
    {
        await Clients.Group(sessionId).SendAsync("ForceLogout", message);
    }
}
