using Microsoft.AspNetCore.Mvc;

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
            ViewData["Message"] = "Whiteboard Demo [ASP.NET + SignalR + Orleans] Core";

            return View();
        }

        public IActionResult Streaming()
        {
            ViewData["Message"] = "Streaming Demo [ASP.NET + SignalR + Orleans] Core";

            return View();
        }
    }
}