using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Contracts
{
    public interface ITownService : IService
    {
        Task<ServiceInteraction> AddTown(string county, string townName);

        Task<ServiceInteraction<string[]>> GetTowns(string county);
    }
}