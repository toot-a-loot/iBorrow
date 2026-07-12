using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
