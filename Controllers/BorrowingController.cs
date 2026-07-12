using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    public class BorrowingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
