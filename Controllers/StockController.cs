using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using ECOMMERCE_NEXOSOFT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(1, 3)]
    public class StockController : Controller
    {
        private readonly NexosoftDbContext _context;

        public StockController(NexosoftDbContext context)
        {
            _context = context;
        }

        // GET: Stock
        public async Task<IActionResult> Index(string? buscar, bool? soloStockBajo)
        {
            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada para gestionar stock.";
                return RedirectToAction("Index", "Vendedor");
            }

            var query = _context.Stocks
                .Include(s => s.IdProductoNavigation)
                    .ThenInclude(p => p.IdCategoriaNavigation)
                .Where(s => s.IdProductoNavigation != null && s.IdProductoNavigation.IdTienda == idTienda.Value)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();
                query = query.Where(s => s.IdProductoNavigation != null &&
                                         s.IdProductoNavigation.NombreProducto.ToLower().Contains(texto));
            }

            if (soloStockBajo == true)
            {
                query = query.Where(s => s.StockActual <= s.StockMinimo);
            }

            var stocks = await query
                .OrderBy(s => s.IdProductoNavigation!.NombreProducto)
                .ToListAsync();

            ViewBag.Busqueda = buscar;
            ViewBag.SoloStockBajo = soloStockBajo;

            return View(stocks);
        }

        // GET: Stock/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction(nameof(Index));
            }

            var stock = await _context.Stocks
                .Include(s => s.IdProductoNavigation)
                .FirstOrDefaultAsync(s => s.IdInventario == id && s.IdProductoNavigation != null && s.IdProductoNavigation.IdTienda == idTienda.Value);

            if (stock == null)
            {
                return NotFound();
            }

            return View(stock);
        }

        // POST: Stock/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdInventario,CodInventario,IdProducto,PrecioCompraStock,StockActual,StockMinimo")] Stock stock)
        {
            if (id != stock.IdInventario)
            {
                return NotFound();
            }

            ModelState.Remove("IdProductoNavigation");

            if (ModelState.IsValid)
            {
                try
                {
                    var idTienda = ObtenerIdTiendaVendedorLogueado();

                    if (idTienda == null)
                    {
                        TempData["MensajeError"] = "No tienes una tienda asociada.";
                        return View(stock);
                    }

                    var stockExistente = await _context.Stocks
                        .Include(s => s.IdProductoNavigation)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.IdInventario == id &&
                                                  s.IdProductoNavigation != null &&
                                                  s.IdProductoNavigation.IdTienda == idTienda.Value);

                    if (stockExistente == null)
                    {
                        return NotFound();
                    }

                    stock.IdProducto = stockExistente.IdProducto;

                    _context.Update(stock);
                    await _context.SaveChangesAsync();
                    TempData["MensajeExito"] = "Stock actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Stocks.Any(e => e.IdInventario == stock.IdInventario))
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            stock.IdProductoNavigation = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == stock.IdProducto) ?? new Producto();

            return View(stock);
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