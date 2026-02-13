using Microsoft.AspNetCore.SignalR;

namespace EnergyAnalyzer.Hubs
{
    public class HasanoglanIpsiHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
