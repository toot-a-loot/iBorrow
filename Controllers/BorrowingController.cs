using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    [Authorize]
    public class BorrowingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
