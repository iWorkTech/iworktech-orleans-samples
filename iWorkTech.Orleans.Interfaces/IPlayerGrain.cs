using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using Orleans;

namespace iWorkTech.Orleans.Interfaces
{
    /// <summary>
    ///     Interface to an individual player that may or may not be in a game at any point in time
    /// </summary>
    public interface IPlayerGrain : IGrainWithGuidKey
    {
        Task JoinGame(IGameGrain game);
        Task LeaveGame(IGameGrain game);
        Task<IGameGrain> GetCurrentGame();
        Task<PairingSummary[]> GetAvailableGames();
        Task<List<GameSummary>> GetGameSummaries();
        Task<Guid> CreateGame();
        Task<GameState> JoinGame(Guid gameId);
        Task LeaveGame(Guid gameId, GameOutcome outcome);
        Task SetUsername(string username);
        Task<string> GetUsername();
    }
}