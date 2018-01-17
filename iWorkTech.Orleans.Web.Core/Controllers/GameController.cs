using System;
using System.Threading.Tasks;
using iWorkTech.Orleans.Common;
using iWorkTech.Orleans.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace iWorkTech.Orleans.Web.Core.Controllers
{
    public class GameController : Controller
    {
        private readonly IGrainFactory _factory;

        public GameController(IGrainFactory factory)
        {
            _factory = factory;
        }

        private Guid GetGuid()
        {
            if (Request.Cookies["playerId"] != null)
                return Guid.Parse(Request.Cookies["playerId"]);
            var guid = Guid.NewGuid();
            Response.Cookies.Append("playerId", guid.ToString());
            return guid;
        }

        public async Task<IActionResult> Index()
        {
            var guid = GetGuid();
            var player = _factory.GetGrain<IPlayerGrain>(guid);
            var gamesTask = player.GetGameSummaries();
            var availableTask = player.GetAvailableGames();
            await Task.WhenAny(gamesTask, availableTask);

            return Json(new object[] {gamesTask.Result, availableTask.Result});
        }

        public async Task<IActionResult> CreateGame()
        {
            var guid = GetGuid();
            var player = _factory.GetGrain<IPlayerGrain>(guid);
            var gameIdTask = await player.CreateGame();
            return Json(new {GameId = gameIdTask});
        }

        public async Task<IActionResult> Join(Guid id)
        {
            var guid = GetGuid();
            var player = _factory.GetGrain<IPlayerGrain>(guid);
            var state = await player.JoinGame(id);
            return Json(new {GameState = state});
        }

        public async Task<IActionResult> GetMoves(Guid id)
        {
            var guid = GetGuid();
            var game = _factory.GetGrain<IGameGrain>(id);
            var moves = await game.GetMoves();
            var summary = await game.GetSummary(guid);
            return Json(new {moves, summary});
        }

        [HttpPost]
        public async Task<IActionResult> MakeMove(Guid id, int x, int y)
        {
            var guid = GetGuid();
            var game = _factory.GetGrain<IGameGrain>(id);
            var move = new GameMove {PlayerId = guid, X = x, Y = y};
            var state = await game.MakeMove(move);
            return Json(state);
        }

        public async Task<IActionResult> QueryGame(Guid id)
        {
            var game = _factory.GetGrain<IGameGrain>(id);
            var state = await game.GetState();
            return Json(state);
        }

        [HttpPost]
        public async Task<IActionResult> SetUser(string id)
        {
            var guid = GetGuid();
            var player = _factory.GetGrain<IPlayerGrain>(guid);
            await player.SetUsername(id);
            return Json(new { });
        }
    }
}