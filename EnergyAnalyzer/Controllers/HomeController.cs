using Microsoft.AspNetCore.Mvc;

namespace EnergyAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IsikYuvar()
        {
            return View();
        }
    }
}
