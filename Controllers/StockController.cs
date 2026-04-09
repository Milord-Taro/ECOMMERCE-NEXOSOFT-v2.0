using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using ECOMMERCE_NEXOSOFT.Models;
using ECOMMERCE_NEXOSOFT.ViewModels;
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

            var rolInterno = ObtenerRolInternoActual(idTienda.Value);

            if (rolInterno == "vendedor_tienda")
            {
                TempData["MensajeError"] = "Tu rol interno no tiene permisos para gestionar inventario.";
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

        //  Stock/Movimientos
        public async Task<IActionResult> Movimientos(string? buscar, string? tipo)
        {
            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada para consultar movimientos.";
                return RedirectToAction("Index", "Vendedor");
            }

            var rolInterno = ObtenerRolInternoActual(idTienda.Value);

            if (rolInterno == "vendedor_tienda")
            {
                TempData["MensajeError"] = "Tu rol interno no tiene permisos para gestionar inventario.";
                return RedirectToAction("Index", "Vendedor");
            }

            var query = _context.MovimientoInventarios
                .Include(m => m.IdProductoNavigation)
                .Include(m => m.IdUsuarioNavigation)
                .Where(m => m.IdProductoNavigation != null && m.IdProductoNavigation.IdTienda == idTienda.Value)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();
                query = query.Where(m =>
                    m.IdProductoNavigation != null &&
                    m.IdProductoNavigation.NombreProducto.ToLower().Contains(texto));
            }

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                var tipoNormalizado = tipo.Trim().ToLower();
                query = query.Where(m => m.TipoMovimiento.ToLower() == tipoNormalizado);
            }

            var movimientos = await query
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();

            ViewBag.Busqueda = buscar;
            ViewBag.TipoSeleccionado = tipo;

            return View(movimientos);
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

            var rolInterno = ObtenerRolInternoActual(idTienda.Value);

            if (rolInterno == "vendedor_tienda")
            {
                TempData["MensajeError"] = "Tu rol interno no tiene permisos para gestionar inventario.";
                return RedirectToAction("Index", "Vendedor");
            }

            var stock = await _context.Stocks
                .Include(s => s.IdProductoNavigation)
                .FirstOrDefaultAsync(s => s.IdInventario == id &&
                                          s.IdProductoNavigation != null &&
                                          s.IdProductoNavigation.IdTienda == idTienda.Value);

            if (stock == null)
            {
                return NotFound();
            }

            var vm = new StockMovimientoViewModel
            {
                IdInventario = stock.IdInventario,
                CodInventario = stock.CodInventario,
                IdProducto = stock.IdProducto,
                NombreProducto = stock.IdProductoNavigation?.NombreProducto ?? "Producto",
                PrecioCompraStock = stock.PrecioCompraStock,
                StockActual = stock.StockActual,
                StockMinimo = stock.StockMinimo,
                TipoMovimiento = string.Empty,
                Cantidad = 0,
                Motivo = string.Empty
            };

            return View(vm);
        }

        // POST: Stock/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StockMovimientoViewModel model)
        {
            if (id != model.IdInventario)
            {
                return NotFound();
            }

            var idTienda = ObtenerIdTiendaVendedorLogueado();
    
            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction(nameof(Index));
            }

            var rolInterno = ObtenerRolInternoActual(idTienda.Value);

            if (rolInterno == "vendedor_tienda")
            {
                TempData["MensajeError"] = "Tu rol interno no tiene permisos para gestionar inventario.";
                return RedirectToAction("Index", "Vendedor");
            }

            var stockExistente = await _context.Stocks
                .Include(s => s.IdProductoNavigation)
                .FirstOrDefaultAsync(s => s.IdInventario == id &&
                                          s.IdProductoNavigation != null &&
                                          s.IdProductoNavigation.IdTienda == idTienda.Value);

            if (stockExistente == null)
            {
                return NotFound();
            }

            model.NombreProducto = stockExistente.IdProductoNavigation?.NombreProducto ?? "Producto";
            model.CodInventario = stockExistente.CodInventario;
            model.IdProducto = stockExistente.IdProducto;
            model.StockActual = stockExistente.StockActual;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tiposValidos = new[]
                {
                "entrada",
                "salida",
                "ajuste_positivo",
                "ajuste_negativo",
                "devolucion",
                "cancelacion"
                };

            if (!tiposValidos.Contains(model.TipoMovimiento))
            {
                ModelState.AddModelError("TipoMovimiento", "Debes seleccionar un tipo de movimiento válido.");
                return View(model);
            }

            if (model.Cantidad <= 0)
            {
                ModelState.AddModelError("Cantidad", "La cantidad debe ser mayor que cero.");
                return View(model);
            }

            int stockAntes = stockExistente.StockActual;
            int stockDespues = stockAntes;

            if (model.TipoMovimiento == "entrada" ||
                model.TipoMovimiento == "ajuste_positivo" ||
                model.TipoMovimiento == "devolucion" ||
                model.TipoMovimiento == "cancelacion")
            {
                stockDespues = stockAntes + model.Cantidad;
            }
            else if (model.TipoMovimiento == "salida" ||
                     model.TipoMovimiento == "ajuste_negativo")
            {
                if (model.Cantidad > stockAntes)
                {
                    ModelState.AddModelError("Cantidad", "La operación no puede superar el stock disponible.");
                    return View(model);
                }

                stockDespues = stockAntes - model.Cantidad;
            }

            if (stockDespues < 0)
            {
                ModelState.AddModelError("Cantidad", "La operación no puede generar stock negativo.");
                return View(model);
            }

            stockExistente.StockMinimo = model.StockMinimo;
            stockExistente.PrecioCompraStock = model.PrecioCompraStock;
            stockExistente.StockActual = stockDespues;

            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            var ultimoCodMovimiento = await _context.MovimientoInventarios
                .OrderByDescending(m => m.IdMovimiento)
                .Select(m => (int?)m.IdMovimiento)
                .FirstOrDefaultAsync();

            var movimiento = new MovimientoInventario
            {
                IdProducto = stockExistente.IdProducto,
                IdUsuario = idUsuario,
                TipoMovimiento = model.TipoMovimiento,
                Cantidad = model.Cantidad,
                StockAnterior = stockAntes,
                StockNuevo = stockDespues,
                Motivo = string.IsNullOrWhiteSpace(model.Motivo) ? null : model.Motivo.Trim(),
                FechaMovimiento = DateTime.Now
            };

            _context.MovimientoInventarios.Add(movimiento);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Movimiento de inventario registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private int? ObtenerIdTiendaVendedorLogueado()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return null;
            }

            // Primero intenta por la nueva estructura: usuario -> miembro_tienda -> tienda
            var idTiendaPorMiembro = _context.MiembroTiendas
                .Where(m => m.IdUsuario == idUsuario.Value)
                .Select(m => (int?)m.IdTienda)
                .FirstOrDefault();

            if (idTiendaPorMiembro != null)
            {
                return idTiendaPorMiembro;
            }

            // Fallback al flujo actual: usuario -> vendedor -> tienda
            var idTienda = _context.Tiendas
                .Include(t => t.IdVendedorNavigation)
                .Where(t => t.IdVendedorNavigation.IdUsuario == idUsuario.Value)
                .Select(t => (int?)t.IdTienda)
                .FirstOrDefault();

            return idTienda;
        }

        private string? ObtenerRolInternoActual(int idTienda)
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return null;
            }

            return _context.MiembroTiendas
                .Include(m => m.IdRolTiendaNavigation)
                .Where(m => m.IdUsuario == idUsuario.Value && m.IdTienda == idTienda)
                .Select(m => m.IdRolTiendaNavigation.NombreRol)
                .FirstOrDefault();
        }
    }
}