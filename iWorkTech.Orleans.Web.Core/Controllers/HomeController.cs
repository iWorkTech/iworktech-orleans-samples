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
    public class HomeController : Controller
    {
        private readonly IGrainFactory _factory;

        public HomeController(IGrainFactory factory)
        {
            _factory = factory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Chat()
        {
            ViewData["Message"] = "Chat Demo [ASP.NET + SignalR + Orleans] Core";

            return View();
        }


        public IActionResult Draw()
        {
            ViewData["Whiteboard"] = "Whiteboard Demo [ASP.NET + SignalR + Orleans] Core";

            return View();
        }

        public IActionResult Streaming()
        {
            ViewData["Message"] = "Streaming Demo [ASP.NET + SignalR + Orleans] Core";

            return View();
        }

        public IActionResult Game(Guid? id)
        {
            ViewData["Message"] = "Game Demo [ASP.NET + SignalR + Orleans] Core";
            var vm = new ViewModel { GameId = (id.HasValue) ? id.Value.ToString() : "" };
            return View(vm);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        private Guid GetGuid()
        {

            if (HttpContext.Session.Get("playerId") != null)
            {
                return new Guid(HttpContext.Session.Get("playerId"));
            }
            var guid = Guid.NewGuid();
            HttpContext.Session.Set("playerId", guid.ToByteArray());
            return guid;
        }

        public class ViewModel
        {
            public string GameId { get; set; }
        }

        public async Task<ActionResult> Join(Guid id)
        {
            var guid = GetGuid();
            var player = _factory.GetGrain<IPlayerGrain>(guid);
            var state = await player.JoinGame(id);
            return RedirectToAction("Game", id);
        }
    }

}
