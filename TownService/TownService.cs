using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TownService
{
    internal sealed class TownService : StatefulService, ITownService
    {
        private const string TownsByCountyStateKey = "TownsByCounty";

        public TownService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task<ServiceInteraction> AddTown(string county, string townName)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var townsByCounty = await GetTownsByCounty();
                await townsByCounty.AddOrUpdateAsync(
                    tx,
                    county,
                    c => new List<string> { townName },
                    (c, t) =>
                    {
                        if (t.Contains(townName))
                            return t;

                        t.Add(townName);
                        return t;
                    });

                await tx.CommitAsync();
            }

            return new ServiceInteraction(this.Partition.PartitionInfo.Id);
        }

        private async Task<IReliableDictionary2<string, List<string>>> GetTownsByCounty()
        {
            return await StateManager.GetOrAddAsync<IReliableDictionary2<string, List<string>>>(TownsByCountyStateKey);
        }

        public async Task<ServiceInteraction<string[]>> GetTowns(string county)
        {
            var townsByCounty = new List<string>();

            using (var tx = StateManager.CreateTransaction())
            {
                var towns = await GetTownsByCounty();

                var townsByCountyEnumerable = await towns.CreateEnumerableAsync(tx, key => key == county, EnumerationMode.Unordered);
                using (var townsByCountyEnumerator = townsByCountyEnumerable.GetAsyncEnumerator())
                {
                    while (await townsByCountyEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        if (townsByCountyEnumerator.Current.Key == county)
                        {
                            townsByCounty = townsByCountyEnumerator.Current.Value;
                        }
                    }
                }
            }

            return new ServiceInteraction<string[]>(this.Partition.PartitionInfo.Id, townsByCounty.ToArray());
        }
    }
}
