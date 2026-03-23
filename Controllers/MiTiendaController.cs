using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(3)]
    public class MiTiendaController : Controller
    {
        private readonly NexosoftDbContext _context;

        public MiTiendaController(NexosoftDbContext context)
        {
            _context = context;
        }

        private async Task<ECOMMERCE_NEXOSOFT.Models.Tienda?> ObtenerTiendaActualAsync()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return null;
            }

            return await _context.Tiendas
                .Include(t => t.IdVendedorNavigation)
                .FirstOrDefaultAsync(t => t.IdVendedorNavigation.IdUsuario == idUsuario.Value);
        }

        public async Task<IActionResult> Index()
        {
            var tienda = await ObtenerTiendaActualAsync();

            if (tienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction("Index", "Vendedor");
            }

            return View(tienda);
        }

        public async Task<IActionResult> Edit()
        {
            var tienda = await ObtenerTiendaActualAsync();

            if (tienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction("Index", "Vendedor");
            }

            return View(tienda);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("IdTienda,CodTienda,IdVendedor,NombreTienda,Descripcion,LogoUrl,VisiblePublico,FechaRegistro")] ECOMMERCE_NEXOSOFT.Models.Tienda tienda)
        {
            var tiendaActual = await ObtenerTiendaActualAsync();

            if (tiendaActual == null || tienda.IdTienda != tiendaActual.IdTienda)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Revisa los datos de la tienda.";
                return View(tienda);
            }

            tienda.NombreTienda = tiendaActual.NombreTienda;
            tienda.IdVendedor = tiendaActual.IdVendedor;
            tienda.CodTienda = tiendaActual.CodTienda;
            tienda.FechaRegistro = tiendaActual.FechaRegistro;

            _context.Update(tienda);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "La información de la tienda se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}