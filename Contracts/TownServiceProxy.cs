using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Contracts
{
    public class TownServiceProxy : ITownService
    {
        private readonly IReadOnlyDictionary<char, string> partitionLookup =
            new Dictionary<char, string>
            {
                { 'A', "A-E" },
                { 'B', "A-E" },
                { 'C', "A-E" },
                { 'D', "A-E" },
                { 'E', "A-E" },
                { 'F', "F-T" },
                { 'G', "F-T" },
                { 'H', "F-T" },
                { 'I', "F-T" },
                { 'J', "F-T" },
                { 'K', "F-T" },
                { 'L', "F-T" },
                { 'M', "F-T" },
                { 'N', "F-T" },
                { 'O', "F-T" },
                { 'P', "F-T" },
                { 'Q', "F-T" },
                { 'R', "F-T" },
                { 'S', "F-T" },
                { 'T', "F-T" },
                { 'U', "U" },
                { 'V', "V" },
                { 'W', "W" },
                { 'X', "X" },
                { 'Y', "Y" },
                { 'Z', "Z" }
            };

        private const string UnknownPartitionName = "MISCELLANEOUS";

        public async Task<ServiceInteraction> AddTown(string county, string townName)
        {
            var townService = GetTownServiceProxy(county);
            return await townService.AddTown(county, townName);
        }

        private ITownService GetTownServiceProxy(string county)
        {
            var serviceUri = new Uri($"fabric:/ServiceFabricApplication/TownService");
            var proxyFactory = new ServiceProxyFactory(_ => new FabricTransportServiceRemotingClientFactory());
            var partitionKey = ApplyPartitioningStrategy(county);
            var townService = proxyFactory.CreateServiceProxy<ITownService>(serviceUri, partitionKey);
            return townService;
        }

        private ServicePartitionKey ApplyPartitioningStrategy(string sourceValue)
        {
            var firstCharacter = sourceValue.ToUpperInvariant().First();
            var partitionName = TownServiceProxy.UnknownPartitionName;

            if (partitionLookup.ContainsKey(firstCharacter))
                partitionName = partitionLookup[firstCharacter];

            return new ServicePartitionKey(partitionName);
        }

        public async Task<ServiceInteraction<string[]>> GetTowns(string county)
        {
            var townService = GetTownServiceProxy(county);
            return await townService.GetTowns(county);
        }
    }
}