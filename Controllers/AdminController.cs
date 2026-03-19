using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(1)]
    public class AdminController : Controller
    {
        private readonly NexosoftDbContext _context;

        public AdminController(NexosoftDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProductos = await _context.Productos.CountAsync();
            ViewBag.ProductosPublicados = await _context.Productos.CountAsync(p => p.VisiblePublico);
            ViewBag.TotalStockBajo = await _context.Stocks.CountAsync(s => s.StockActual <= s.StockMinimo);
            ViewBag.TotalPedidos = await _context.Pedidos.CountAsync();
            ViewBag.TotalUsuarios = await _context.Usuarios.CountAsync();
            ViewBag.TotalClientes = await _context.Usuarios.CountAsync(u => u.IdRol == 2);
            ViewBag.TotalVendedores = await _context.Usuarios.CountAsync(u => u.IdRol == 3);

            ViewBag.StockCritico = await _context.Stocks
                .Include(s => s.IdProductoNavigation)
                .OrderBy(s => s.StockActual)
                .Take(5)
                .ToListAsync();

            ViewBag.UltimosPedidos = await _context.Pedidos
                .OrderByDescending(p => p.FechaCreacion)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}