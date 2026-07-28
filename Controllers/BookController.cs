using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iBorrow.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        public IActionResult Details(int? id)
        {
            return View();
        }
    }
}
