using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Orleans;

namespace iWorkTech.Orleans.Grains
{
    /// <summary>
    ///     Represents an individual player that may or may not be in a game at any point in time
    /// </summary>
    public class PlayerGrain : Grain, IPlayerGrain
    {
        private IGameGrain currentGame;
        private int gamesStarted;
        private int loses;
        private string username;
        private int wins;
        private List<Guid> ListOfActiveGames { get; set; }
        private List<Guid> ListOfPastGames { get; set; }

        // Game the player is currently in. May be null.
        public Task<IGameGrain> GetCurrentGame()
        {
            return Task.FromResult(currentGame);
        }

        // Game grain calls this method to notify that the player has joined the game.
        public Task JoinGame(IGameGrain game)
        {
            currentGame = game;
            Console.WriteLine("Player {0} joined game {1}", this.GetPrimaryKey(), game.GetPrimaryKey());
            return Task.CompletedTask;
        }

        // Game grain calls this method to notify that the player has left the game.
        public Task LeaveGame(IGameGrain game)
        {
            currentGame = null;
            Console.WriteLine("Player {0} left game {1}", this.GetPrimaryKey(), game.GetPrimaryKey());
            return Task.CompletedTask;
        }

        public async Task<PairingSummary[]> GetAvailableGames()
        {
            var grain = GrainFactory.GetGrain<IPairingGrain>(0);
            return (await grain.GetGames()).Where(x => !ListOfActiveGames.Contains(x.GameId)).ToArray();
        }

        // create a new game, and add oursleves to that game
        public async Task<Guid> CreateGame()
        {
            gamesStarted += 1;

            var gameId = Guid.NewGuid();
            var gameGrain = GrainFactory.GetGrain<IGameGrain>(gameId); // create new game

            // add ourselves to the game
            var playerId = this.GetPrimaryKey(); // our player id
            await gameGrain.AddPlayerToGame(playerId);
            ListOfActiveGames.Add(gameId);
            var name = username + "'s " + AddOrdinalSuffix(gamesStarted.ToString()) + " game";
            await gameGrain.SetName(name);

            var pairingGrain = GrainFactory.GetGrain<IPairingGrain>(0);
            await pairingGrain.AddGame(gameId, name);

            return gameId;
        }


        // join a game that is awaiting players
        public async Task<GameState> JoinGame(Guid gameId)
        {
            var gameGrain = GrainFactory.GetGrain<IGameGrain>(gameId);

            var state = await gameGrain.AddPlayerToGame(this.GetPrimaryKey());
            ListOfActiveGames.Add(gameId);

            var pairingGrain = GrainFactory.GetGrain<IPairingGrain>(0);
            await pairingGrain.RemoveGame(gameId);

            return state;
        }


        // leave game when it is over
        public Task LeaveGame(Guid gameId, GameOutcome outcome)
        {
            // manage game list

            ListOfActiveGames.Remove(gameId);
            ListOfPastGames.Add(gameId);

            // manage running total

            if (outcome == GameOutcome.Win)
                wins++;
            if (outcome == GameOutcome.Lose)
                loses++;

            return Task.CompletedTask;
        }

        public async Task<List<GameSummary>> GetGameSummaries()
        {
            var tasks = new List<Task<GameSummary>>();
            foreach (var gameId in ListOfActiveGames)
            {
                var game = GrainFactory.GetGrain<IGameGrain>(gameId);
                tasks.Add(game.GetSummary(this.GetPrimaryKey()));
            }
            await Task.WhenAll(tasks);
            return tasks.Select(x => x.Result).ToList();
        }

        public Task SetUsername(string name)
        {
            username = name;
            return Task.CompletedTask;
        }

        public Task<string> GetUsername()
        {
            return Task.FromResult(username);
        }

        public override Task OnActivateAsync()
        {
            currentGame = null;
            ListOfActiveGames = new List<Guid>();
            ListOfPastGames = new List<Guid>();

            wins = 0;
            loses = 0;
            gamesStarted = 0;

            return Task.CompletedTask;
        }


        private static string AddOrdinalSuffix(string number)
        {
            var n = int.Parse(number);
            var nMod100 = n % 100;

            if (nMod100 >= 11 && nMod100 <= 13)
                return string.Concat(number, "th");

            switch (n % 10)
            {
                case 1:
                    return string.Concat(number, "st");
                case 2:
                    return string.Concat(number, "nd");
                case 3:
                    return string.Concat(number, "rd");
                default:
                    return string.Concat(number, "th");
            }
        }
    }
}