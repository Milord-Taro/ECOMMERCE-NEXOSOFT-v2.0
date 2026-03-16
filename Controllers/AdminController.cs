using Microsoft.AspNetCore.Mvc;
using ECOMMERCE_NEXOSOFT.Filters;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(1)] // 🔒 SOLO ADMIN
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}