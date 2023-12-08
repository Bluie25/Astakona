using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class Hubs : Hub
{
    public async Task SendOrderEntry()
    {
        await Clients.All.SendAsync("ReceiveOrderEntry");
    }

    public async Task SendOrderUpdate()
    {
        await Clients.All.SendAsync("ReceiveOrderUpdate");
    }

    public async Task SendOrderDelete() 
    {
        await Clients.All.SendAsync("ReceiveOrderDelete");
    }

    public async Task SendScrewUpdate()
    {
        await Clients.All.SendAsync("ReceiveScrewUpdate");
    }
}