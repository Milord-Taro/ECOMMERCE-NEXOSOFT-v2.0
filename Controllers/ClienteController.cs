using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(2)]
    public class ClienteController : Controller
    {
        private readonly NexosoftDbContext _context;

        public ClienteController(NexosoftDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewBag.TotalPedidos = await _context.Pedidos.CountAsync(p => p.IdUsuario == idUsuario.Value);

            ViewBag.UltimosPedidos = await _context.Pedidos
                .Where(p => p.IdUsuario == idUsuario.Value)
                .OrderByDescending(p => p.FechaCreacion)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}