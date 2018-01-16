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
 
        public IActionResult Game()
        {
            ViewData["Message"] = "Games Demo [ASP.NET + SignalR + Orleans] Core";

            return View();
        }


    }

}
