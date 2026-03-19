using Microsoft.AspNetCore.Mvc;
using ECOMMERCE_NEXOSOFT.Filters;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(3)] // 🧑‍💼 SOLO VENDEDOR
    public class VendedorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
