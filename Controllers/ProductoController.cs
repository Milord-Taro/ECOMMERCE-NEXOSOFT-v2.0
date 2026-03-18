using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Models;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    public class ProductoController : Controller
    {
        private readonly NexosoftDbContext _context;

        public ProductoController(NexosoftDbContext context)
        {
            _context = context;
        }

        // GET: Producto
        public async Task<IActionResult> Index(string? buscar, int? categoria, bool? soloStockBajo)
        {
            var query = _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .Include(p => p.Stock)
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

            if (soloStockBajo == true)
            {
                query = query.Where(p =>
                    p.Stock != null &&
                    p.Stock.StockActual <= p.Stock.StockMinimo);
            }

            var productos = await query
                .OrderBy(p => p.NombreProducto)
                .ToListAsync();

            ViewBag.Categorias = await _context.Categoria
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();

            ViewBag.Busqueda = buscar;
            ViewBag.CategoriaSeleccionada = categoria;
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
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "NombreCategoria");

            ViewData["UnidadMedidaProducto"] = new SelectList(
                new List<string> { "Unidad", "Caja", "Paquete", "Metro", "Litro", "Kilogramo" }
            );

            return View();
        }

        // POST: Producto/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProducto,CodProducto,IdCategoria,NombreProducto,DescripcionCorta,SkuProducto,CodigoBarrasProducto,UnidadMedidaProducto,MarcaProducto,Favorito,VisiblePublico,PrecioVentaProducto")] Producto producto)
        {
            ModelState.Remove("IdCategoriaNavigation");
            ModelState.Remove("Detallepedidos");
            ModelState.Remove("Stock");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(producto);
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

            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "NombreCategoria", producto.IdCategoria);

            ViewData["UnidadMedidaProducto"] = new SelectList(
                new List<string> { "Unidad", "Caja", "Paquete", "Metro", "Litro", "Kilogramo" },
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

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "NombreCategoria", producto.IdCategoria);

            ViewData["UnidadMedidaProducto"] = new SelectList(
                new List<string> { "Unidad", "Caja", "Paquete", "Metro", "Litro", "Kilogramo" },
                producto.UnidadMedidaProducto
            );

            return View(producto);
        }

        // POST: Producto/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProducto,CodProducto,IdCategoria,NombreProducto,DescripcionCorta,SkuProducto,CodigoBarrasProducto,UnidadMedidaProducto,MarcaProducto,Favorito,VisiblePublico,PrecioVentaProducto")] Producto producto)
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

            ViewData["IdCategoria"] = new SelectList(_context.Categoria, "IdCategoria", "NombreCategoria", producto.IdCategoria);

            ViewData["UnidadMedidaProducto"] = new SelectList(
                new List<string> { "Unidad", "Caja", "Paquete", "Metro", "Litro", "Kilogramo" },
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
    }
}
