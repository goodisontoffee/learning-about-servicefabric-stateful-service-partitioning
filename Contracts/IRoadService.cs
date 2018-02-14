using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

[assembly: FabricTransportServiceRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace Contracts
{
    public interface IRoadService : IService
    {
        Task<ServiceInteraction> AddRoad(string town, string roadName);

        Task<ServiceInteraction<string[]>> GetRoads(string town);
    }
}
