using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class Hubs : Hub
{
    public async Task SendOrdersPageUpdate()
    {
        await Clients.All.SendAsync("ReceiveOrdersPageUpdate");
    }

    public async Task SendMaterialsPageUpdate()
    {
        await Clients.All.SendAsync("ReceiveMaterialsPageUpdate");
    }

    public async Task SendPalletsPageUpdate()
    {
        await Clients.All.SendAsync("ReceivePalletsPageUpdate");
    }

    public async Task SendDeliveriesPageUpdate()
    {
        await Clients.All.SendAsync("ReceiveDeliveriesPageUpdate");
    }

    public async Task SendAccountsPageUpdate()
    {
        await Clients.All.SendAsync("ReceiveAccountsPageUpdate");
    }
}