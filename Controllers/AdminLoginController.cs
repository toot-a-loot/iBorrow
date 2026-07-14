using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class AdminLoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
