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
using ECOMMERCE_NEXOSOFT.Helpers;
using System.Text.RegularExpressions;

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
    .Select(c => new Categorium
    {
        IdCategoria = c.IdCategoria,
        CodCategoria = c.CodCategoria,
        NombreCategoria = c.NombreCategoria ?? string.Empty,
        Descripcion = c.Descripcion ?? string.Empty,
        VisiblePublico = c.VisiblePublico
    })
    .OrderBy(c => c.NombreCategoria)
    .ToListAsync();

            ViewBag.Subcategorias = await _context.Subcategoria
    .Where(s => !categoria.HasValue || s.IdCategoria == categoria.Value)
    .Select(s => new Subcategorium
    {
        IdSubcategoria = s.IdSubcategoria,
        CodSubcategoria = s.CodSubcategoria,
        IdCategoria = s.IdCategoria,
        NombreSubcategoria = s.NombreSubcategoria ?? string.Empty,
        Descripcion = s.Descripcion ?? string.Empty,
        VisiblePublico = s.VisiblePublico
    })
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

            var rolInterno = ObtenerRolInternoActual(idTienda.Value);

            if (rolInterno == "vendedor_tienda")
            {
                TempData["MensajeError"] = "Tu rol interno no tiene permisos para gestionar productos.";
                return RedirectToAction(nameof(Index));
            }

            CargarCombosProducto();
            return View(new Producto { VisiblePublico = true });
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

            NormalizarProducto(producto);
            await ValidarProductoAsync(producto);

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

                    var rolInterno = ObtenerRolInternoActual(idTienda.Value);

                    if (rolInterno == "vendedor_tienda")
                    {
                        TempData["MensajeError"] = "Tu rol interno no tiene permisos para gestionar productos.";
                        return RedirectToAction(nameof(Index));
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

            var rolInterno = ObtenerRolInternoActual(idTienda.Value);

            if (rolInterno == "vendedor_tienda")
            {
                TempData["MensajeError"] = "Tu rol interno no tiene permisos para gestionar productos.";
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

            NormalizarProducto(producto);
            await ValidarProductoAsync(producto, producto.IdProducto);

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

                    var rolInterno = ObtenerRolInternoActual(idTienda.Value);

                    if (rolInterno == "vendedor_tienda")
                    {
                        TempData["MensajeError"] = "Tu rol interno no tiene permisos para gestionar productos.";
                        return RedirectToAction(nameof(Index));
                    }

                    var productoExistente = await _context.Productos
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.IdProducto == id && p.IdTienda == idTienda.Value);

                    if (productoExistente == null)
                    {
                        return NotFound();
                    }

                    producto.IdTienda = productoExistente.IdTienda;

                    var productoActual = await _context.Productos
                        .FirstOrDefaultAsync(p => p.IdProducto == id && p.IdTienda == idTienda.Value);

                    if (productoActual == null)
                    {
                        return NotFound();
                    }

                    productoActual.CodProducto = producto.CodProducto;
                    productoActual.IdCategoria = producto.IdCategoria;
                    productoActual.IdSubcategoria = producto.IdSubcategoria;
                    productoActual.NombreProducto = producto.NombreProducto;
                    productoActual.DescripcionCorta = producto.DescripcionCorta;
                    productoActual.SkuProducto = producto.SkuProducto;
                    productoActual.CodigoBarrasProducto = producto.CodigoBarrasProducto;
                    productoActual.UnidadMedidaProducto = producto.UnidadMedidaProducto;
                    productoActual.MarcaProducto = producto.MarcaProducto;
                    productoActual.Favorito = producto.Favorito;
                    productoActual.VisiblePublico = producto.VisiblePublico;
                    productoActual.PrecioVentaProducto = producto.PrecioVentaProducto;

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

            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction(nameof(Index));
            }

            var producto = await _context.Productos
                .Include(p => p.IdCategoriaNavigation)
                .FirstOrDefaultAsync(m => m.IdProducto == id && m.IdTienda == idTienda.Value);

            if (producto == null)
            {
                return NotFound();
            }

            if (producto.IdTienda == null)
            {
                TempData["MensajeError"] = "El producto no tiene una tienda asociada válida.";
                return RedirectToAction(nameof(Index));
            }

            var rolInterno = ObtenerRolInternoActual(producto.IdTienda.Value);

            if (rolInterno == "vendedor_tienda")
            {
                TempData["MensajeError"] = "Tu rol interno no tiene permisos para ocultar productos.";
                return RedirectToAction(nameof(Index));
            }

            return View(producto);
        }

        // POST: Producto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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
                TempData["MensajeError"] = "No se encontró el producto.";
                return RedirectToAction(nameof(Index));
            }

            if (producto.IdTienda == null)
            {
                TempData["MensajeError"] = "El producto no tiene una tienda asociada válida.";
                return RedirectToAction(nameof(Index));
            }

            var rolInterno = ObtenerRolInternoActual(producto.IdTienda.Value);

            if (rolInterno == "vendedor_tienda")
            {
                TempData["MensajeError"] = "Tu rol interno no tiene permisos para ocultar productos.";
                return RedirectToAction(nameof(Index));
            }

            producto.VisiblePublico = false;
            _context.Update(producto);
            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = "Producto ocultado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private void NormalizarProducto(Producto producto)
        {
            producto.NombreProducto = InputNormalizer.NormalizeProductName(producto.NombreProducto);
            producto.DescripcionCorta = string.IsNullOrWhiteSpace(producto.DescripcionCorta)
                ? null
                : InputNormalizer.NormalizeText(producto.DescripcionCorta);

            producto.SkuProducto = InputNormalizer.NormalizeSku(producto.SkuProducto);
            producto.CodigoBarrasProducto = InputNormalizer.NormalizeBarcode(producto.CodigoBarrasProducto);

            producto.MarcaProducto = string.IsNullOrWhiteSpace(producto.MarcaProducto)
                ? null
                : InputNormalizer.NormalizeBrand(producto.MarcaProducto);

            producto.UnidadMedidaProducto = InputNormalizer.NormalizeText(producto.UnidadMedidaProducto).ToLowerInvariant();
        }

        private async Task ValidarProductoAsync(Producto producto, int idActual = 0)
        {
            if (producto.IdCategoria <= 0)
            {
                ModelState.AddModelError("IdCategoria", "La categoría es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(producto.NombreProducto))
            {
                ModelState.AddModelError("NombreProducto", "El nombre del producto es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(producto.DescripcionCorta))
            {
                ModelState.AddModelError("DescripcionCorta", "La descripción corta es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(producto.MarcaProducto))
            {
                ModelState.AddModelError("MarcaProducto", "La marca es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(producto.UnidadMedidaProducto))
            {
                ModelState.AddModelError("UnidadMedidaProducto", "La unidad de medida es obligatoria.");
            }

            var skuExiste = await _context.Productos.AnyAsync(p =>
                p.IdProducto != idActual &&
                p.SkuProducto != null &&
                p.SkuProducto.Trim().ToUpper() == producto.SkuProducto.Trim().ToUpper());

            if (skuExiste)
            {
                ModelState.AddModelError("SkuProducto", "Ya existe un producto con ese SKU.");
            }

            var codigoBarrasExiste = await _context.Productos.AnyAsync(p =>
                p.IdProducto != idActual &&
                p.CodigoBarrasProducto != null &&
                p.CodigoBarrasProducto.Trim() == producto.CodigoBarrasProducto.Trim());

            if (codigoBarrasExiste)
            {
                ModelState.AddModelError("CodigoBarrasProducto", "Ya existe un producto con ese código de barras.");
            }
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
