using Orleans;
using System.Threading.Tasks;

namespace iWorkTech.Orleans.Interfaces
{
    public interface ITokenGrain : IGrainWithStringKey
    {
        Task<AuthTokenState> Get();
        Task Create(AuthTokenState value);
        Task Revoke();
    }
}
