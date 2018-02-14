using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace RoadService
{
    internal sealed class RoadService : StatefulService, IRoadService
    {
        private const string RoadsByTownStateKey = "RoadsByTown";

        public RoadService(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        public async Task<ServiceInteraction> AddRoad(string town, string roadName)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var roadsByTown = await GetRoadsByTown();
                await roadsByTown.AddOrUpdateAsync(
                    tx,
                    town,
                    t => new List<string> { roadName },
                    (t, r) =>
                    {
                        if (r.Contains(roadName))
                            return r;

                        r.Add(roadName);
                        return r;
                    });

                await tx.CommitAsync();
            }

            return new ServiceInteraction(this.Partition.PartitionInfo.Id);
        }

        private async Task<IReliableDictionary2<string, List<string>>> GetRoadsByTown()
        {
            return await StateManager.GetOrAddAsync<IReliableDictionary2<string, List<string>>>(RoadsByTownStateKey);
        }

        public async Task<ServiceInteraction<string[]>> GetRoads(string town)
        {
            var roadsByTown = new List<string>();

            using (var tx = StateManager.CreateTransaction())
            {
                var roads = await GetRoadsByTown();

                var roadsByTownEnumerable = await roads.CreateEnumerableAsync(tx, key => key == town, EnumerationMode.Unordered);
                using (var roadsByTownEnumerator = roadsByTownEnumerable.GetAsyncEnumerator())
                {
                    while (await roadsByTownEnumerator.MoveNextAsync(CancellationToken.None))
                    {
                        if (roadsByTownEnumerator.Current.Key == town)
                        {
                            roadsByTown = roadsByTownEnumerator.Current.Value;
                        }
                    }
                }
            }

            return new ServiceInteraction<string[]>(this.Partition.PartitionInfo.Id, roadsByTown.ToArray());
        }
    }
}
