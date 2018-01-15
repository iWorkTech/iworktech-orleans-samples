using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using Orleans;

namespace iWorkTech.Orleans.Interfaces
{
    /// <summary>
    ///     Interface to a specific instance of a game with its own status and list of players
    /// </summary>
    public interface IGameGrain : IGrainWithGuidKey
    {
        Task UpdateGameStatus(GameStatus status);
        Task SubscribeForGameUpdates(IGameObserver subscriber);
        Task UnsubscribeForGameUpdates(IGameObserver subscriber);
        Task<GameState> AddPlayerToGame(Guid player);
        Task<GameState> GetState();
        Task<List<GameMove>> GetMoves();
        Task<GameState> MakeMove(GameMove move);
        Task<GameSummary> GetSummary(Guid player);
        Task SetName(string name);
    }
}