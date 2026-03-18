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

        public async Task<IActionResult> Index(
    int? categoria,
    string? buscar,
    string? marca,
    string? orden,
    bool? soloDisponibles)
        {
            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.Stock)
                .Where(p => p.VisiblePublico)
                .AsQueryable();

            if (categoria.HasValue)
            {
                query = query.Where(p => p.IdCategoria == categoria.Value);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();

                query = query.Where(p =>
                    p.NombreProducto.ToLower().Contains(texto) ||
                    (p.DescripcionCorta != null && p.DescripcionCorta.ToLower().Contains(texto)) ||
                    (p.MarcaProducto != null && p.MarcaProducto.ToLower().Contains(texto)) ||
                    p.IdCategoriaNavigation.NombreCategoria.ToLower().Contains(texto));
            }

            if (!string.IsNullOrWhiteSpace(marca))
            {
                var marcaTexto = marca.Trim().ToLower();

                query = query.Where(p =>
                    p.MarcaProducto != null &&
                    p.MarcaProducto.ToLower() == marcaTexto);
            }

            if (soloDisponibles == true)
            {
                query = query.Where(p => p.Stock != null && p.Stock.StockActual > 0);
            }

            query = orden switch
            {
                "nombre_desc" => query.OrderByDescending(p => p.NombreProducto),
                "precio_asc" => query.OrderBy(p => p.PrecioVentaProducto),
                "precio_desc" => query.OrderByDescending(p => p.PrecioVentaProducto),
                _ => query.OrderBy(p => p.NombreProducto)
            };

            var productos = await query.ToListAsync();

            var categorias = await _context.Categoria
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();

            var marcas = await _context.Productos
                .Where(p => p.VisiblePublico &&
                            p.MarcaProducto != null &&
                            p.MarcaProducto != "")
                .Select(p => p.MarcaProducto!)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            ViewBag.Categorias = categorias;
            ViewBag.Marcas = marcas;
            ViewBag.CategoriaSeleccionada = categoria;
            ViewBag.Busqueda = buscar;
            ViewBag.MarcaSeleccionada = marca;
            ViewBag.OrdenSeleccionado = orden;
            ViewBag.SoloDisponibles = soloDisponibles;

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