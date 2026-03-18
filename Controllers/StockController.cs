using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Models;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
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
            var query = _context.Stocks
                .Include(s => s.IdProductoNavigation)
                .ThenInclude(p => p.IdCategoriaNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();

                query = query.Where(s =>
                    s.IdProductoNavigation.NombreProducto.ToLower().Contains(texto));
            }

            if (soloStockBajo == true)
            {
                query = query.Where(s => s.StockActual <= s.StockMinimo);
            }

            var stocks = await query
                .OrderBy(s => s.IdProductoNavigation.NombreProducto)
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

            var stock = await _context.Stocks
                .Include(s => s.IdProductoNavigation)
                .FirstOrDefaultAsync(s => s.IdInventario == id);

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
    }
}