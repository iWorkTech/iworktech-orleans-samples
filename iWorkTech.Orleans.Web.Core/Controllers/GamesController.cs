using System;
using System.Diagnostics;
using System.Threading.Tasks;
using iWorkTech.Orleans.Interfaces;
using iWorkTech.Orleans.Web.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace iWorkTech.Orleans.Web.Core.Controllers
{
    public class GamesController : Controller
    {
        private readonly IGrainFactory _factory;

        public GamesController(IGrainFactory factory)
        {
            _factory = factory;
        }

        public IActionResult Index(Guid? id)
        {
            var vm = new ViewModel {GameId = id.HasValue ? id.Value.ToString() : ""};
            return View(vm);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        private Guid GetGuid()
        {
            if (HttpContext.Session.Get("playerId") != null)
                return new Guid(HttpContext.Session.Get("playerId"));
            var guid = Guid.NewGuid();
            HttpContext.Session.Set("playerId", guid.ToByteArray());
            return guid;
        }

        public async Task<ActionResult> Join(Guid id)
        {
            var guid = GetGuid();
            var player = _factory.GetGrain<IPlayerGrain>(guid);
            var state = await player.JoinGame(id);
            return RedirectToAction("Index", id);
        }

        public class ViewModel
        {
            public string GameId { get; set; }
        }
    }
}