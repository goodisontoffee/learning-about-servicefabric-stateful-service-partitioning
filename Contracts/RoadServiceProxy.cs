using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Contracts
{
    public class RoadServiceProxy : IRoadService
    {
        public async Task<ServiceInteraction> AddRoad(string town, string roadName)
        {
            var roadService = GetRoadServiceProxy(town);
            return await roadService.AddRoad(town, roadName);
        }

        private IRoadService GetRoadServiceProxy(string town)
        {
            var serviceUri = new Uri($"fabric:/ServiceFabricApplication/RoadService");
            var proxyFactory = new ServiceProxyFactory(_ => new FabricTransportServiceRemotingClientFactory());
            var partitionKey = ApplyPartitioningStrategy(town);
            var roadService = proxyFactory.CreateServiceProxy<IRoadService>(serviceUri, partitionKey);
            return roadService;
        }

        private ServicePartitionKey ApplyPartitioningStrategy(string sourceValue)
        {
            return new ServicePartitionKey(sourceValue.Length);
        }

        public async Task<ServiceInteraction<string[]>> GetRoads(string town)
        {
            var roadService = GetRoadServiceProxy(town);
            return await roadService.GetRoads(town);
        }
    }
}
