using ECOMMERCE_NEXOSOFT.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    public class TiendasController : Controller
    {
        private readonly NexosoftDbContext _context;

        public TiendasController(NexosoftDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tienda = await _context.Tiendas
                .Include(t => t.Productos.Where(p => p.VisiblePublico))
                    .ThenInclude(p => p.IdCategoriaNavigation)
                .Include(t => t.Productos.Where(p => p.VisiblePublico))
                    .ThenInclude(p => p.IdSubcategoriaNavigation)
                .FirstOrDefaultAsync(t => t.IdTienda == id && t.VisiblePublico);

            if (tienda == null)
            {
                return NotFound();
            }

            return View(tienda);
        }
    }
}