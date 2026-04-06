using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using ECOMMERCE_NEXOSOFT.Helpers;
using ECOMMERCE_NEXOSOFT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(1)]
    public class SubcategoriaController : Controller
    {
        private readonly NexosoftDbContext _context;

        public SubcategoriaController(NexosoftDbContext context)
        {
            _context = context;
        }

        private void CargarCategorias(int? categoriaSeleccionada = null)
        {
            ViewData["IdCategoria"] = new SelectList(
                _context.Categoria.OrderBy(c => c.NombreCategoria),
                "IdCategoria",
                "NombreCategoria",
                categoriaSeleccionada);
        }

        private void NormalizarSubcategoria(Subcategorium subcategorium)
        {
            subcategorium.NombreSubcategoria = InputNormalizer.NormalizeText(subcategorium.NombreSubcategoria);
            subcategorium.Descripcion = InputNormalizer.NormalizeText(subcategorium.Descripcion);
        }

        private async Task ValidarSubcategoriaAsync(Subcategorium subcategorium, int idActual = 0)
        {
            var nombreNormalizado = InputNormalizer.NormalizeText(subcategorium.NombreSubcategoria).ToLower();
            var descripcionNormalizada = InputNormalizer.NormalizeText(subcategorium.Descripcion);

            if (subcategorium.IdCategoria <= 0)
            {
                ModelState.AddModelError("IdCategoria", "La categoría es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                ModelState.AddModelError("NombreSubcategoria", "El nombre de la subcategoría es obligatorio.");
                return;
            }

            if (string.IsNullOrWhiteSpace(descripcionNormalizada))
            {
                ModelState.AddModelError("Descripcion", "La descripción de la subcategoría es obligatoria.");
            }

            var nombreExiste = await _context.Subcategoria.AnyAsync(s =>
                s.IdSubcategoria != idActual &&
                s.NombreSubcategoria != null &&
                s.NombreSubcategoria.Trim().ToLower() == nombreNormalizado);

            if (nombreExiste)
            {
                ModelState.AddModelError("NombreSubcategoria", "Ya existe una subcategoría con ese nombre.");
            }
        }

        public async Task<IActionResult> Index(string? buscar, int? categoria, bool? visible)
        {
            var query = _context.Subcategoria
                .Include(s => s.IdCategoriaNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();

                query = query.Where(s =>
                    s.NombreSubcategoria.ToLower().Contains(texto) ||
                    s.CodSubcategoria.ToString().Contains(texto));
            }

            if (categoria.HasValue)
            {
                query = query.Where(s => s.IdCategoria == categoria.Value);
            }

            if (visible.HasValue)
            {
                query = query.Where(s => s.VisiblePublico == visible.Value);
            }

            var subcategorias = await query
                .Select(s => new Subcategorium
                {
                    IdSubcategoria = s.IdSubcategoria,
                    CodSubcategoria = s.CodSubcategoria,
                    IdCategoria = s.IdCategoria,
                    NombreSubcategoria = s.NombreSubcategoria ?? string.Empty,
                    Descripcion = s.Descripcion ?? string.Empty,
                    VisiblePublico = s.VisiblePublico,
                    IdCategoriaNavigation = s.IdCategoriaNavigation == null
                        ? null!
                        : new Categorium
                        {
                            IdCategoria = s.IdCategoriaNavigation.IdCategoria,
                            CodCategoria = s.IdCategoriaNavigation.CodCategoria,
                            NombreCategoria = s.IdCategoriaNavigation.NombreCategoria ?? string.Empty,
                            Descripcion = s.IdCategoriaNavigation.Descripcion ?? string.Empty,
                            VisiblePublico = s.IdCategoriaNavigation.VisiblePublico
                        }
                })
                .OrderBy(s => s.NombreSubcategoria)
                .ToListAsync();

            ViewBag.CategoriasFiltro = await _context.Categoria
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.CategoriaSeleccionada = categoria;
            ViewBag.Visible = visible;

            return View(subcategorias);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var subcategoria = await _context.Subcategoria
                .Include(s => s.IdCategoriaNavigation)
                .Where(s => s.IdSubcategoria == id)
                .Select(s => new Subcategorium
                {
                    IdSubcategoria = s.IdSubcategoria,
                    CodSubcategoria = s.CodSubcategoria,
                    IdCategoria = s.IdCategoria,
                    NombreSubcategoria = s.NombreSubcategoria ?? string.Empty,
                    Descripcion = s.Descripcion ?? string.Empty,
                    VisiblePublico = s.VisiblePublico,
                    IdCategoriaNavigation = s.IdCategoriaNavigation == null
                        ? null!
                        : new Categorium
                        {
                            IdCategoria = s.IdCategoriaNavigation.IdCategoria,
                            CodCategoria = s.IdCategoriaNavigation.CodCategoria,
                            NombreCategoria = s.IdCategoriaNavigation.NombreCategoria ?? string.Empty,
                            Descripcion = s.IdCategoriaNavigation.Descripcion ?? string.Empty,
                            VisiblePublico = s.IdCategoriaNavigation.VisiblePublico
                        }
                })
                .FirstOrDefaultAsync();

            if (subcategoria == null) return NotFound();

            return View(subcategoria);
        }

        public IActionResult Create()
        {
            CargarCategorias();
            return View(new Subcategorium { VisiblePublico = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSubcategoria,CodSubcategoria,IdCategoria,NombreSubcategoria,Descripcion,VisiblePublico")] Subcategorium subcategorium)
        {
            ModelState.Remove("IdCategoriaNavigation");
            ModelState.Remove("Productos");

            NormalizarSubcategoria(subcategorium);
            await ValidarSubcategoriaAsync(subcategorium);

            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Revisa los datos obligatorios de la subcategoría.";
                CargarCategorias(subcategorium.IdCategoria);
                return View(subcategorium);
            }

            try
            {
                _context.Add(subcategorium);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Subcategoría creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["MensajeError"] = "No fue posible guardar la subcategoría. Verifica que el código no esté repetido y que los datos sean válidos.";
                CargarCategorias(subcategorium.IdCategoria);
                return View(subcategorium);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subcategoria = await _context.Subcategoria.FindAsync(id);
            if (subcategoria == null) return NotFound();

            CargarCategorias(subcategoria.IdCategoria);
            return View(subcategoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSubcategoria,CodSubcategoria,IdCategoria,NombreSubcategoria,Descripcion,VisiblePublico")] Subcategorium subcategorium)
        {
            if (id != subcategorium.IdSubcategoria) return NotFound();

            ModelState.Remove("IdCategoriaNavigation");
            ModelState.Remove("Productos");

            NormalizarSubcategoria(subcategorium);
            await ValidarSubcategoriaAsync(subcategorium, subcategorium.IdSubcategoria);

            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Revisa los datos obligatorios de la subcategoría.";
                CargarCategorias(subcategorium.IdCategoria);
                return View(subcategorium);
            }

            try
            {
                var subcategoriaActual = await _context.Subcategoria.FindAsync(id);
                if (subcategoriaActual == null) return NotFound();

                subcategoriaActual.CodSubcategoria = subcategorium.CodSubcategoria;
                subcategoriaActual.IdCategoria = subcategorium.IdCategoria;
                subcategoriaActual.NombreSubcategoria = subcategorium.NombreSubcategoria;
                subcategoriaActual.Descripcion = subcategorium.Descripcion;
                subcategoriaActual.VisiblePublico = subcategorium.VisiblePublico;

                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Subcategoría actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["MensajeError"] = "No fue posible actualizar la subcategoría. Verifica los datos ingresados.";
                CargarCategorias(subcategorium.IdCategoria);
                return View(subcategorium);
            }
        }

        public IActionResult Delete(int? id)
        {
            TempData["MensajeExito"] = "La eliminación de subcategorías está deshabilitada en esta versión.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            TempData["MensajeExito"] = "La eliminación de subcategorías está deshabilitada en esta versión.";
            return RedirectToAction(nameof(Index));
        }
    }
}