using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
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

        public async Task<IActionResult> Index()
        {
            var subcategorias = await _context.Subcategoria
                .Include(s => s.IdCategoriaNavigation)
                .OrderBy(s => s.NombreSubcategoria)
                .ToListAsync();

            return View(subcategorias);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var subcategoria = await _context.Subcategoria
                .Include(s => s.IdCategoriaNavigation)
                .FirstOrDefaultAsync(s => s.IdSubcategoria == id);

            if (subcategoria == null) return NotFound();

            return View(subcategoria);
        }

        public IActionResult Create()
        {
            ViewData["IdCategoria"] = new SelectList(_context.Categoria.OrderBy(c => c.NombreCategoria), "IdCategoria", "NombreCategoria");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSubcategoria,CodSubcategoria,IdCategoria,NombreSubcategoria,Descripcion,VisiblePublico")] Subcategorium subcategorium)
        {

            ModelState.Remove("IdCategoriaNavigation");
            ModelState.Remove("Productos");

            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Revisa los campos obligatorios de la subcategoría.";
                ViewData["IdCategoria"] = new SelectList(_context.Categoria.OrderBy(c => c.NombreCategoria), "IdCategoria", "NombreCategoria", subcategorium.IdCategoria);
                return View(subcategorium);
            }

            try
            {
                _context.Add(subcategorium);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Subcategoría creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "No fue posible guardar la subcategoría. Verifica que el código no esté repetido y que los datos sean válidos.";
                ViewData["IdCategoria"] = new SelectList(_context.Categoria.OrderBy(c => c.NombreCategoria), "IdCategoria", "NombreCategoria", subcategorium.IdCategoria);
                return View(subcategorium);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subcategoria = await _context.Subcategoria.FindAsync(id);
            if (subcategoria == null) return NotFound();

            ViewData["IdCategoria"] = new SelectList(_context.Categoria.OrderBy(c => c.NombreCategoria), "IdCategoria", "NombreCategoria", subcategoria.IdCategoria);
            return View(subcategoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSubcategoria,CodSubcategoria,IdCategoria,NombreSubcategoria,Descripcion,VisiblePublico")] Subcategorium subcategorium)
        {
            if (id != subcategorium.IdSubcategoria) return NotFound();

            ModelState.Remove("IdCategoriaNavigation");
            ModelState.Remove("Productos");

            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Revisa los campos obligatorios de la subcategoría.";
                ViewData["IdCategoria"] = new SelectList(_context.Categoria.OrderBy(c => c.NombreCategoria), "IdCategoria", "NombreCategoria", subcategorium.IdCategoria);
                return View(subcategorium);
            }

            try
            {
                _context.Update(subcategorium);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Subcategoría actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "No fue posible actualizar la subcategoría. Verifica los datos ingresados.";
                ViewData["IdCategoria"] = new SelectList(_context.Categoria.OrderBy(c => c.NombreCategoria), "IdCategoria", "NombreCategoria", subcategorium.IdCategoria);
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