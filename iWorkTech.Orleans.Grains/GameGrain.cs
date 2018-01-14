using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using iWorkTech.Orleans.Interfaces;
using Orleans;

namespace iWorkTech.Orleans.Grains
{
    /// <summary>
    ///     Represents a game in progress and holds the game's state in memory.
    ///     Notifies player grains about their players joining and leaving the game.
    ///     Updates subscribed observers about the progress of the game
    /// </summary>
    public class GameGrain : Grain, IGameGrain
    {
        private HashSet<Guid> _players;
        private GameStatus _status;
        private ObserverSubscriptionManager<IGameObserver> _subscribers;

        /// <summary>
        ///     Presense grain calls this method to update the game with its latest status
        /// </summary>
        public async Task UpdateGameStatus(GameStatus status)
        {
            this._status = status;

            // Check for new players that joined since last update
            foreach (var player in status.Players)
                if (!_players.Contains(player))
                    try
                    {
                        // Here we call player grains serially, which is less efficient than a fan-out but simpler to express.
                        await GrainFactory.GetGrain<IPlayerGrain>(player).JoinGame(this);
                        _players.Add(player);
                    }
                    catch (Exception)
                    {
                        // Ignore exceptions while telling player grains to join the game. 
                        // Since we didn't add the player to the list, this will be tried again with next update.
                    }

            // Check for players that left the game since last update
            var promises = new List<Task>();
            foreach (var player in _players)
                if (!status.Players.Contains(player))
                    try
                    {
                        // Here we do a fan-out with multiple calls going out in parallel. We join the promisses later.
                        // More code to write but we get lower latency when calling multiple player grains.
                        promises.Add(GrainFactory.GetGrain<IPlayerGrain>(player).LeaveGame(this));
                        _players.Remove(player);
                    }
                    catch (Exception)
                    {
                        // Ignore exceptions while telling player grains to leave the game.
                        // Since we didn't remove the player from the list, this will be tried again with next update.
                    }

            // Joining promises
            await Task.WhenAll(promises);

            // Notify subsribers about the latest game score
            _subscribers.Notify(s => s.UpdateGameScore(status.Score));
        }

        public Task SubscribeForGameUpdates(IGameObserver subscriber)
        {
            _subscribers.Subscribe(subscriber);
            return Task.CompletedTask;
        }

        public Task UnsubscribeForGameUpdates(IGameObserver subscriber)
        {
            _subscribers.Unsubscribe(subscriber);
            return Task.CompletedTask;
        }

        public override Task OnActivateAsync()
        {
            _subscribers = new ObserverSubscriptionManager<IGameObserver>();
            _players = new HashSet<Guid>();
            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            _subscribers.Clear();
            _players.Clear();
            return Task.CompletedTask;
        }
    }
}