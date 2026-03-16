using ECOMMERCE_NEXOSOFT.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    public class ProductosController : Controller
    {
        private readonly NexosoftDbContext _context;

        public ProductosController(NexosoftDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoria)
        {
            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .AsQueryable();

            if (categoria.HasValue)
            {
                query = query.Where(p => p.IdCategoria == categoria.Value);
            }

            var productos = await query.ToListAsync();

            ViewBag.CategoriaSeleccionada = categoria;

            return View(productos);
        }

        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }
    }
}