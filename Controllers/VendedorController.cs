using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(3)]
    public class VendedorController : Controller
    {
        private readonly NexosoftDbContext _context;

        public VendedorController(NexosoftDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return View();
            }

            ViewBag.TotalProductos = await _context.Productos.CountAsync(p => p.IdTienda == idTienda.Value);
            ViewBag.ProductosPublicados = await _context.Productos.CountAsync(p => p.IdTienda == idTienda.Value && p.VisiblePublico);
            ViewBag.ProductosOcultos = await _context.Productos.CountAsync(p => p.IdTienda == idTienda.Value && !p.VisiblePublico);
            ViewBag.TotalStockBajo = await _context.Stocks.CountAsync(s =>
                s.IdProductoNavigation != null &&
                s.IdProductoNavigation.IdTienda == idTienda.Value &&
                s.StockActual <= s.StockMinimo);

            ViewBag.StockCritico = await _context.Stocks
                .Include(s => s.IdProductoNavigation)
                .Where(s => s.IdProductoNavigation != null && s.IdProductoNavigation.IdTienda == idTienda.Value)
                .OrderBy(s => s.StockActual)
                .Take(5)
                .ToListAsync();

            return View();
        }

        private int? ObtenerIdTiendaVendedorLogueado()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return null;
            }

            var idTienda = _context.Tiendas
                .Include(t => t.IdVendedorNavigation)
                .Where(t => t.IdVendedorNavigation.IdUsuario == idUsuario.Value && t.VisiblePublico)
                .Select(t => (int?)t.IdTienda)
                .FirstOrDefault();

            return idTienda;
        }
    }
}