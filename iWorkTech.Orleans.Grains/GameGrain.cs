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
    ///     Represents a game in progress and holds the game's state in memory.
    ///     Notifies player grains about their players joining and leaving the game.
    ///     Updates subscribed observers about the progress of the game
    /// </summary>
    public class GameGrain : Grain, IGameGrain
    {
        private HashSet<Guid> _players;
        private GameStatus _status;
        private ObserverSubscriptionManager<IGameObserver> _subscribers;
        private Guid gameId;
        private int indexNextPlayerToMove;
        private string name;
        private int[,] theBoard;

        // list of players in the current game
        // for simplicity, player 0 always plays an "O" and player 1 plays an "X"
        //  who starts a game is a random call once a game is started, and is set via indexNextPlayerToMove
        public List<Guid> ListOfPlayers { get; private set; }

        public GameState GameState { get; private set; }
        public Guid WinnerId { get; private set; } // set when game is over
        public Guid LoserId { get; private set; } // set when game is over

        // we record a game in terms of each of the moves, so we could reconstruct the sequence of play
        // during an active game, we also use a 2D array to represent the board, to make it
        //  easier to check for legal moves, wining lines, etc. 
        //  -1 represents an empty square, 0 & 1 the player's index 
        public List<GameMove> ListOfMoves { get; private set; }


        /// <summary>
        ///     Presense grain calls this method to update the game with its latest status
        /// </summary>
        public async Task UpdateGameStatus(GameStatus status)
        {
            _status = status;

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

        // add a player into a game
        public Task<GameState> AddPlayerToGame(Guid player)
        {
            // check if its ok to join this game
            switch (GameState)
            {
                case GameState.Finished:
                    throw new ApplicationException("Can't join game once its over");
                case GameState.InPlay:
                    throw new ApplicationException("Can't join game once its in play");
            }

            // add player
            ListOfPlayers.Add(player);

            // check if the game is ready to play
            switch (GameState)
            {
                case GameState.AwaitingPlayers when ListOfPlayers.Count == 2:
                    // a new game is starting
                    GameState = GameState.InPlay;
                    indexNextPlayerToMove = new Random().Next(0, 1); // random as to who has the first move
                    break;
            }

            // let user know if game is ready or not
            return Task.FromResult(GameState);
        }


        // make a move during the game
        public async Task<GameState> MakeMove(GameMove move)
        {
            // check if its a legal move to make
            if (GameState != GameState.InPlay) throw new ApplicationException("This game is not in play");

            if (ListOfPlayers.IndexOf(move.PlayerId) < 0)
                throw new ArgumentException("No such playerid for this game", nameof(move));
            if (move.PlayerId != ListOfPlayers[indexNextPlayerToMove])
                throw new ArgumentException("The wrong player tried to make a move", nameof(move));

            if (move.X < 0 || move.X > 2 || move.Y < 0 || move.Y > 2)
                throw new ArgumentException("Bad co-ordinates for a move", nameof(move));
            if (theBoard[move.X, move.Y] != -1) throw new ArgumentException("That square is not empty", nameof(move));

            // record move
            ListOfMoves.Add(move);
            theBoard[move.X, move.Y] = indexNextPlayerToMove;

            // check for a winning move
            var win = false;
            for (var i = 0; i < 3 && !win; i++)
                win = IsWinningLine(theBoard[i, 0], theBoard[i, 1], theBoard[i, 2]);
            if (!win)
                for (var i = 0; i < 3 && !win; i++)
                    win = IsWinningLine(theBoard[0, i], theBoard[1, i], theBoard[2, i]);
            if (!win)
                win = IsWinningLine(theBoard[0, 0], theBoard[1, 1], theBoard[2, 2]);
            if (!win)
                win = IsWinningLine(theBoard[0, 2], theBoard[1, 1], theBoard[2, 0]);

            // check for draw

            var draw = ListOfMoves.Count == 9;

            // handle end of game
            if (win || draw)
            {
                // game over
                GameState = GameState.Finished;
                if (win)
                {
                    WinnerId = ListOfPlayers[indexNextPlayerToMove];
                    LoserId = ListOfPlayers[(indexNextPlayerToMove + 1) % 2];
                }

                // collect tasks up, so we await both notifications at the same time
                var promises = new List<Task>();
                // inform this player of outcome
                var playerGrain = GrainFactory.GetGrain<IPlayerGrain>(ListOfPlayers[indexNextPlayerToMove]);
                promises.Add(playerGrain.LeaveGame(this.GetPrimaryKey(), win ? GameOutcome.Win : GameOutcome.Draw));

                // inform other player of outcome
                playerGrain = GrainFactory.GetGrain<IPlayerGrain>(ListOfPlayers[(indexNextPlayerToMove + 1) % 2]);
                promises.Add(playerGrain.LeaveGame(this.GetPrimaryKey(), win ? GameOutcome.Lose : GameOutcome.Draw));
                await Task.WhenAll(promises);
                return GameState;
            }

            // if game hasnt ended, prepare for next players move
            indexNextPlayerToMove = (indexNextPlayerToMove + 1) % 2;
            return GameState;
        }


        public Task<GameState> GetState()
        {
            return Task.FromResult(GameState);
        }

        public Task<List<GameMove>> GetMoves()
        {
            return Task.FromResult(ListOfMoves);
        }

        public async Task<GameSummary> GetSummary(Guid player)
        {
            var promises = ListOfPlayers.Where(p => p != player).Select(p => GrainFactory.GetGrain<IPlayerGrain>(p).GetUsername()).ToList();
            await Task.WhenAll(promises);

            return new GameSummary
            {
                NumMoves = ListOfMoves.Count,
                State = GameState,
                YourMove = GameState == GameState.InPlay && player == ListOfPlayers[indexNextPlayerToMove],
                NumPlayers = ListOfPlayers.Count,
                GameId = this.GetPrimaryKey(),
                Usernames = promises.Select(x => x.Result).ToArray(),
                Name = name,
                GameStarter = ListOfPlayers.FirstOrDefault() == player
            };
        }

        public Task SetName(string playerName)
        {
            name = playerName;
            return Task.CompletedTask;
        }

        public override Task OnActivateAsync()
        {
            _subscribers = new ObserverSubscriptionManager<IGameObserver>();
            _players = new HashSet<Guid>();

            // make sure newly formed game is in correct state 
            ListOfPlayers = new List<Guid>();
            ListOfMoves = new List<GameMove>();
            indexNextPlayerToMove = -1; // safety default - is set when game begins to 0 or 1
            theBoard = new int[3, 3] {{-1, -1, -1}, {-1, -1, -1}, {-1, -1, -1}}; // -1 is empty

            GameState = GameState.AwaitingPlayers;
            WinnerId = Guid.Empty;
            LoserId = Guid.Empty;

            gameId = this.GetPrimaryKey();

            return Task.CompletedTask;
        }

        public override Task OnDeactivateAsync()
        {
            _subscribers.Clear();
            _players.Clear();

            return Task.CompletedTask;
        }

        private bool IsWinningLine(int i, int j, int k)
        {
            switch (i)
            {
                case 0 when j == 0 && k == 0:
                    return true;
                case 1 when j == 1 && k == 1:
                    return true;
            }

            return false;
        }
    }
}