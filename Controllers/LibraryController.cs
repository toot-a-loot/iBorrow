using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class LibraryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
