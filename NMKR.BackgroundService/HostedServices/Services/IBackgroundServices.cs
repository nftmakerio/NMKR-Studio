using System.Threading;
using System.Threading.Tasks;
using NMKR.Shared.Model;
using MassTransit;
using StackExchange.Redis;

namespace NMKR.BackgroundService.HostedServices.Services
{
    public interface IBackgroundServices
    {

        public Task Execute(EasynftprojectsContext db, CancellationToken cancellationToken,
            int counter,Backgroundserver server, bool mainnet, int serverid, IConnectionMultiplexer redis, IBus bus);
    }
}
