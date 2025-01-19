using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

public class RoomHub : Hub
{
    public async Task JoinRoomGroup(string roomCode)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
    }

    public async Task LeaveRoomGroup(string roomCode)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
    }

    public async Task SendMessage(string roomCode, string userName, string message)
    {
        await Clients.Group(roomCode).SendAsync("ReceiveMessage", userName, message);
    }
}
