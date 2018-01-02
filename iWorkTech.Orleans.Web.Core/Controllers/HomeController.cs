using System.Diagnostics;
using iWorkTech.Orleans.Web.Core.Models;
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
            ViewData["Message"] = "Chat Demo using ASP.NET Signalr Core";

            return View();
        }

        public IActionResult Streaming()
        {
            ViewData["Message"] = "Streaming Demo using ASP.NET Signalr Core";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}