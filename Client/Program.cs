using System;
using Contracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Press a key once the cluster is ready.");
            Console.ReadKey(true);

            AddRoad("London", "Downing Street");
            AddRoadToWrongParition("London", "Bond Street");
            AddRoad("London", "Oxford Street");
            AddRoad("York", "Micklegate");

            ShowRoads("Edinburgh");
            ShowRoads("London");
            ShowRoadsInWrongPartition("London");
            ShowRoads("York");

            Console.ReadKey(true);
        }

        private static ServiceInteraction AddRoad(string town, string roadName)
        {
            var roadService = GetRoadServiceProxy();
            return AddRoad(town, roadName, roadService);
        }

        private static IRoadService GetRoadServiceProxy()
        {
            var serviceUri = new Uri($"fabric:/ServiceFabricApplication/RoadServiceFacade");
            var proxyFactory = new ServiceProxyFactory(_ => new FabricTransportServiceRemotingClientFactory());
            return proxyFactory.CreateServiceProxy<IRoadService>(serviceUri);
        }

        private static ServiceInteraction AddRoad(string town, string roadName, IRoadService roadService)
        {
            Console.WriteLine($"Adding road: {roadName} to town: {town}.");
            var serviceInteraction = roadService.AddRoad(town, roadName).GetAwaiter().GetResult();
            DescribeServiceInteraction(serviceInteraction);
            return serviceInteraction;
        }

        private static void DescribeServiceInteraction(ServiceInteraction serviceInteraction)
        {
            Console.WriteLine($"{serviceInteraction.PartitionId} was enlisted at {serviceInteraction.InteractedAtUtc.ToShortDateString()} {serviceInteraction.InteractedAtUtc.ToShortTimeString()}");
            Console.WriteLine();
        }

        private static ServiceInteraction AddRoadToWrongParition(string town, string roadName)
        {
            Console.WriteLine($"Applying incorrect partitioning strategy.");
            var roadService = ApplyIncorrectPartitioningStrategy(town);
            return AddRoad(town, roadName, roadService);
        }

        private static IRoadService ApplyIncorrectPartitioningStrategy(string town)
        {
            var serviceUri = new Uri($"fabric:/ServiceFabricApplication/RoadService");
            var proxyFactory = new ServiceProxyFactory(_ => new FabricTransportServiceRemotingClientFactory());
            var partitionKey = new ServicePartitionKey(town.Length + 400);
            var roadService = proxyFactory.CreateServiceProxy<IRoadService>(serviceUri, partitionKey);
            return roadService;
        }

        private static ServiceInteraction ShowRoads(string town)
        {
            var roadService = GetRoadServiceProxy();
            return ShowRoads(town, roadService);
        }

        private static ServiceInteraction ShowRoads(string town, IRoadService roadService)
        {
            Console.WriteLine($"Showing roads in town: {town}.");
            var serviceInteraction = roadService.GetRoads(town).GetAwaiter().GetResult();
            DescribeServiceInteraction(serviceInteraction, "road");
            return serviceInteraction;
        }

        private static void DescribeServiceInteraction(ServiceInteraction<string[]> serviceInteraction, string description)
        {
            foreach (var value in serviceInteraction.Result)
            {
                Console.WriteLine($"Discovered {description}: {value}.");
            }

            DescribeServiceInteraction(serviceInteraction as ServiceInteraction);
        }

        private static ServiceInteraction ShowRoadsInWrongPartition(string town)
        {
            Console.WriteLine($"Applying incorrect partitioning strategy.");
            var roadService = ApplyIncorrectPartitioningStrategy(town);
            return ShowRoads(town, roadService);
        }
    }
}