using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using Orleans;

namespace iWorkTech.Orleans.Interfaces
{
    public interface IPairingGrain : IGrainWithIntegerKey
    {
        Task AddGame(Guid gameId, string name);
        Task RemoveGame(Guid gameId);
        Task<PairingSummary[]> GetGames();
    }
}