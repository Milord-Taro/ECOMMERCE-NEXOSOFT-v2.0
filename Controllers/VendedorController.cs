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
            ViewBag.TotalProductos = await _context.Productos.CountAsync();
            ViewBag.ProductosPublicados = await _context.Productos.CountAsync(p => p.VisiblePublico);
            ViewBag.ProductosOcultos = await _context.Productos.CountAsync(p => !p.VisiblePublico);
            ViewBag.TotalStockBajo = await _context.Stocks.CountAsync(s => s.StockActual <= s.StockMinimo);

            ViewBag.StockCritico = await _context.Stocks
                .Include(s => s.IdProductoNavigation)
                .OrderBy(s => s.StockActual)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}