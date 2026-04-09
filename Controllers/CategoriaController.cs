using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using ECOMMERCE_NEXOSOFT.Helpers;
using ECOMMERCE_NEXOSOFT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(1)]
    public class CategoriaController : Controller
    {
        private readonly NexosoftDbContext _context;

        public CategoriaController(NexosoftDbContext context)
        {
            _context = context;
        }

        private void NormalizarCategoria(Categorium categorium)
        {
            categorium.NombreCategoria = InputNormalizer.NormalizeText(categorium.NombreCategoria);
            categorium.Descripcion = InputNormalizer.NormalizeText(categorium.Descripcion);
        }

        private async Task ValidarCategoriaAsync(Categorium categorium, int idActual = 0)
        {
            var nombreNormalizado = InputNormalizer.NormalizeText(categorium.NombreCategoria).ToLower();
            var descripcionNormalizada = InputNormalizer.NormalizeText(categorium.Descripcion);

            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                ModelState.AddModelError("NombreCategoria", "El nombre de la categoría es obligatorio.");
                return;
            }

            if (string.IsNullOrWhiteSpace(descripcionNormalizada))
            {
                ModelState.AddModelError("Descripcion", "La descripción de la categoría es obligatoria.");
            }

            var nombreExiste = await _context.Categoria.AnyAsync(c =>
                c.IdCategoria != idActual &&
                c.NombreCategoria != null &&
                c.NombreCategoria.Trim().ToLower() == nombreNormalizado);

            if (nombreExiste)
            {
                ModelState.AddModelError("NombreCategoria", "Ya existe una categoría con ese nombre.");
            }
        }

        public async Task<IActionResult> Index(string? buscar, bool? visible)
        {
            var query = _context.Categoria.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();

                query = query.Where(c =>
                    c.NombreCategoria.ToLower().Contains(texto) ||
                    c.CodCategoria.ToString().Contains(texto));
            }

            if (visible.HasValue)
            {
                query = query.Where(c => c.VisiblePublico == visible.Value);
            }

            var categorias = await query
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.Visible = visible;

            return View(categorias);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var categorium = await _context.Categoria
                .FirstOrDefaultAsync(m => m.IdCategoria == id);

            if (categorium == null) return NotFound();

            return View(categorium);
        }

        public IActionResult Create()
        {
            return View(new Categorium { VisiblePublico = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCategoria,CodCategoria,NombreCategoria,Descripcion,VisiblePublico")] Categorium categorium)
        {
            ModelState.Remove("Productos");
            ModelState.Remove("Subcategoria");

            NormalizarCategoria(categorium);
            await ValidarCategoriaAsync(categorium);

            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Revisa los datos obligatorios de la categoría.";
                return View(categorium);
            }

            try
            {
                _context.Add(categorium);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Categoría creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["MensajeError"] = "No fue posible guardar la categoría. Verifica que el código no esté repetido y que los datos sean válidos.";
                return View(categorium);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var categorium = await _context.Categoria.FindAsync(id);
            if (categorium == null) return NotFound();

            return View(categorium);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCategoria,CodCategoria,NombreCategoria,Descripcion,VisiblePublico")] Categorium categorium)
        {
            if (id != categorium.IdCategoria) return NotFound();

            ModelState.Remove("Productos");
            ModelState.Remove("Subcategoria");

            NormalizarCategoria(categorium);
            await ValidarCategoriaAsync(categorium, categorium.IdCategoria);

            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Revisa los datos obligatorios de la categoría.";
                return View(categorium);
            }

            try
            {
                var categoriaActual = await _context.Categoria.FindAsync(id);
                if (categoriaActual == null) return NotFound();

                categoriaActual.CodCategoria = categorium.CodCategoria;
                categoriaActual.NombreCategoria = categorium.NombreCategoria;
                categoriaActual.Descripcion = categorium.Descripcion;
                categoriaActual.VisiblePublico = categorium.VisiblePublico;

                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Categoría actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoriumExists(categorium.IdCategoria))
                {
                    return NotFound();
                }

                TempData["MensajeError"] = "No fue posible actualizar la categoría.";
                return View(categorium);
            }
            catch
            {
                TempData["MensajeError"] = "No fue posible actualizar la categoría. Verifica los datos ingresados.";
                return View(categorium);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var categorium = await _context.Categoria
                .FirstOrDefaultAsync(m => m.IdCategoria == id);

            if (categorium == null) return NotFound();

            return View(categorium);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categorium = await _context.Categoria.FindAsync(id);

            if (categorium != null)
            {
                categorium.VisiblePublico = false;
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Categoría ocultada correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoriumExists(int id)
        {
            return _context.Categoria.Any(e => e.IdCategoria == id);
        }
    }
}