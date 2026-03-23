using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Models;
using ECOMMERCE_NEXOSOFT.Filters;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(1, 3)]
    public class ProductoController : Controller
    {
        private readonly NexosoftDbContext _context;

        public ProductoController(NexosoftDbContext context)
        {
            _context = context;
        }

        // GET: Producto
        public async Task<IActionResult> Index(string? buscar, int? categoria, int? subcategoria, bool? soloStockBajo)
        {
            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada para gestionar productos.";
                return RedirectToAction("Index", "Vendedor");
            }

            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdSubcategoriaNavigation)
                .Include(p => p.IdTiendaNavigation)
                .Include(p => p.Stock)
                .Where(p => p.IdTienda == idTienda.Value)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();

                query = query.Where(p =>
                    p.NombreProducto.ToLower().Contains(texto) ||
                    (p.MarcaProducto != null && p.MarcaProducto.ToLower().Contains(texto)) ||
                    p.CodProducto.ToString().Contains(texto));
            }

            if (categoria.HasValue)
            {
                query = query.Where(p => p.IdCategoria == categoria.Value);
            }

            if (subcategoria.HasValue)
            {
                query = query.Where(p => p.IdSubcategoria == subcategoria.Value);
            }

            if (soloStockBajo == true)
            {
                query = query.Where(p => p.Stock != null && p.Stock.StockActual <= p.Stock.StockMinimo);
            }

            var productos = await query
                .OrderBy(p => p.NombreProducto)
                .ToListAsync();

            ViewBag.Categorias = await _context.Categoria
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();

            ViewBag.Subcategorias = await _context.Subcategoria
                .Where(s => !categoria.HasValue || s.IdCategoria == categoria.Value)
                .OrderBy(s => s.NombreSubcategoria)
                .ToListAsync();

            ViewBag.Busqueda = buscar;
            ViewBag.CategoriaSeleccionada = categoria;
            ViewBag.SubcategoriaSeleccionada = subcategoria;
            ViewBag.SoloStockBajo = soloStockBajo;

            return View(productos);
        }

        // GET: Producto/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.IdSubcategoriaNavigation)
                .Include(p => p.IdTiendaNavigation)
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Producto/Create
        public IActionResult Create()
        {
            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada para crear productos.";
                return RedirectToAction(nameof(Index));
            }

            CargarCombosProducto();
            return View();
        }

        // POST: Producto/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProducto,CodProducto,IdCategoria,IdSubcategoria,NombreProducto,DescripcionCorta,SkuProducto,CodigoBarrasProducto,UnidadMedidaProducto,MarcaProducto,Favorito,VisiblePublico,PrecioVentaProducto")] Producto producto)
        {
            ModelState.Remove("IdCategoriaNavigation");
            ModelState.Remove("Detallepedidos");
            ModelState.Remove("Stock");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(producto);

                    var idTienda = ObtenerIdTiendaVendedorLogueado();

                    if (idTienda == null)
                    {
                        TempData["MensajeError"] = "No tienes una tienda asociada para crear productos.";
                        CargarCombosProducto(producto.IdCategoria, producto.IdSubcategoria);
                        return View(producto);
                    }

                    producto.IdTienda = idTienda.Value;

                    await _context.SaveChangesAsync();

                    var ultimoCodInventario = await _context.Stocks
                        .OrderByDescending(s => s.CodInventario)
                        .Select(s => (int?)s.CodInventario)
                        .FirstOrDefaultAsync();

                    int nuevoCodInventario = (ultimoCodInventario ?? 5000) + 1;

                    var stock = new Stock
                    {
                        CodInventario = nuevoCodInventario,
                        IdProducto = producto.IdProducto,
                        PrecioCompraStock = 0,
                        StockActual = 0,
                        StockMinimo = 0
                    };

                    _context.Stocks.Add(stock);
                    await _context.SaveChangesAsync();

                    TempData["MensajeExito"] = "Producto creado correctamente con stock inicial en 0.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar el producto: " + ex.Message);
                }
            }

            CargarCombosProducto(producto.IdCategoria, producto.IdSubcategoria);
            ViewData["UnidadMedidaProducto"] = new SelectList(
                new List<string> { "unidad", "caja", "paquete", "metro", "metro_cuadrado", "litro", "galon", "kilogramo" },
                producto.UnidadMedidaProducto
            );

            return View(producto);
        }

        // GET: Producto/Edit/5
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

            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.IdProducto == id && p.IdTienda == idTienda.Value);

            if (producto == null)
            {
                return NotFound();
            }

            CargarCombosProducto(producto.IdCategoria, producto.IdSubcategoria);
            ViewData["UnidadMedidaProducto"] = new SelectList(
                new List<string> { "unidad", "caja", "paquete", "metro", "metro_cuadrado", "litro", "galon", "kilogramo" },
                producto.UnidadMedidaProducto
            );

            return View(producto);
        }

        // POST: Producto/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProducto,CodProducto,IdCategoria,IdSubcategoria,NombreProducto,DescripcionCorta,SkuProducto,CodigoBarrasProducto,UnidadMedidaProducto,MarcaProducto,Favorito,VisiblePublico,PrecioVentaProducto")] Producto producto)
        {
            if (id != producto.IdProducto)
            {
                return NotFound();
            }

            ModelState.Remove("IdCategoriaNavigation");
            ModelState.Remove("Detallepedidos");
            ModelState.Remove("Stock");

            if (ModelState.IsValid)
            {
                try
                {
                    var idTienda = ObtenerIdTiendaVendedorLogueado();

                    if (idTienda == null)
                    {
                        TempData["MensajeError"] = "No tienes una tienda asociada.";
                        CargarCombosProducto(producto.IdCategoria, producto.IdSubcategoria);
                        return View(producto);
                    }

                    var productoExistente = await _context.Productos
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.IdProducto == id && p.IdTienda == idTienda.Value);

                    if (productoExistente == null)
                    {
                        return NotFound();
                    }

                    producto.IdTienda = productoExistente.IdTienda;

                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["MensajeExito"] = "Producto actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.IdProducto))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            CargarCombosProducto(producto.IdCategoria, producto.IdSubcategoria);
            ViewData["UnidadMedidaProducto"] = new SelectList(
                new List<string> { "unidad", "caja", "paquete", "metro", "metro_cuadrado", "litro", "galon", "kilogramo" },
                producto.UnidadMedidaProducto
            );

            return View(producto);
        }


        // GET: Producto/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.IdProducto == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Producto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto != null)
            {
                producto.VisiblePublico = false;
                _context.Update(producto);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Producto ocultado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.IdProducto == id);
        }

        private void CargarCombosProducto(int? idCategoriaSeleccionada = null, int? idSubcategoriaSeleccionada = null)
        {
            var categorias = _context.Categoria
                .Where(c => c.VisiblePublico || c.IdCategoria == idCategoriaSeleccionada)
                .OrderBy(c => c.NombreCategoria)
                .ToList();

            var subcategorias = _context.Subcategoria
                .Where(s =>
                    (s.VisiblePublico || s.IdSubcategoria == idSubcategoriaSeleccionada) &&
                    (!idCategoriaSeleccionada.HasValue || s.IdCategoria == idCategoriaSeleccionada.Value))
                .OrderBy(s => s.NombreSubcategoria)
                .ToList();

            ViewData["IdCategoria"] = new SelectList(categorias, "IdCategoria", "NombreCategoria", idCategoriaSeleccionada);
            ViewData["IdSubcategoria"] = new SelectList(subcategorias, "IdSubcategoria", "NombreSubcategoria", idSubcategoriaSeleccionada);

            ViewData["UnidadMedidaProducto"] = new SelectList(
                new List<string> { "unidad", "caja", "paquete", "metro", "metro_cuadrado", "litro", "galon", "kilogramo" }
            );
        }

        [HttpGet]
        public JsonResult ObtenerSubcategorias(int idCategoria)
        {
            var subcategorias = _context.Subcategoria
                .Where(s => s.VisiblePublico && s.IdCategoria == idCategoria)
                .OrderBy(s => s.NombreSubcategoria)
                .Select(s => new
                {
                    idSubcategoria = s.IdSubcategoria,
                    nombreSubcategoria = s.NombreSubcategoria
                })
                .ToList();

            return Json(subcategorias);
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
