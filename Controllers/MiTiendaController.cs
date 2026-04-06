using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECOMMERCE_NEXOSOFT.Helpers;
using System.Text.RegularExpressions;

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

            // Primero intenta por la nueva estructura: usuario -> miembro_tienda -> tienda
            var tiendaPorMiembro = await _context.MiembroTiendas
                .Include(m => m.IdTiendaNavigation)
                .Where(m => m.IdUsuario == idUsuario.Value)
                .Select(m => m.IdTiendaNavigation)
                .FirstOrDefaultAsync();

            if (tiendaPorMiembro != null)
            {
                return tiendaPorMiembro;
            }

            // Fallback al flujo actual: usuario -> vendedor -> tienda
            return await _context.Tiendas
                .Include(t => t.IdVendedorNavigation)
                .FirstOrDefaultAsync(t => t.IdVendedorNavigation.IdUsuario == idUsuario.Value);
        }

        private async Task<string?> ObtenerRolInternoActualAsync(int idTienda)
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return null;
            }

            return await _context.MiembroTiendas
                .Include(m => m.IdRolTiendaNavigation)
                .Where(m => m.IdUsuario == idUsuario.Value && m.IdTienda == idTienda)
                .Select(m => m.IdRolTiendaNavigation.NombreRol)
                .FirstOrDefaultAsync();
        }

        private void NormalizarTienda(ECOMMERCE_NEXOSOFT.Models.Tienda tienda)
        {
            tienda.NombreTienda = InputNormalizer.NormalizeStoreName(tienda.NombreTienda);
            tienda.Descripcion = string.IsNullOrWhiteSpace(tienda.Descripcion)
                ? null
                : InputNormalizer.NormalizeText(tienda.Descripcion);

            tienda.LogoUrl = string.IsNullOrWhiteSpace(tienda.LogoUrl)
                ? null
                : InputNormalizer.NormalizeText(tienda.LogoUrl);
        }

        private void ValidarTienda(ECOMMERCE_NEXOSOFT.Models.Tienda tienda, int idTiendaActual)
        {
            if (!Regex.IsMatch(tienda.NombreTienda ?? string.Empty, ValidationRules.StoreNamePattern))
            {
                ModelState.AddModelError("NombreTienda", "El nombre de la tienda debe tener entre 2 y 30 caracteres y solo puede contener letras, números, espacios, &, - y .");
            }

            var nombreNormalizado = (tienda.NombreTienda ?? string.Empty).Trim().ToLower();

            var nombreExiste = _context.Tiendas.Any(t =>
                t.IdTienda != idTiendaActual &&
                t.NombreTienda.Trim().ToLower() == nombreNormalizado);

            if (nombreExiste)
            {
                ModelState.AddModelError("NombreTienda", "Ya existe una tienda con ese nombre.");
            }
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

            var rolInterno = await ObtenerRolInternoActualAsync(tienda.IdTienda);

            if (rolInterno != "admin_tienda")
            {
                TempData["MensajeError"] = "Solo el admin_tienda puede editar la información de la tienda.";
                return RedirectToAction(nameof(Index));
            }

            return View(tienda);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("IdTienda,CodTienda,IdVendedor,NombreTienda,Descripcion,LogoUrl,VisiblePublico,FechaRegistro,RazonSocial,NitRut,NombreRepresentante,TelefonoContacto,CorreoContacto,DireccionComercial")] ECOMMERCE_NEXOSOFT.Models.Tienda tienda)
        {
            ModelState.Remove("IdVendedorNavigation");
            ModelState.Remove("Pedidos");
            ModelState.Remove("Productos");

            var tiendaActual = await ObtenerTiendaActualAsync();

            if (tiendaActual == null || tienda.IdTienda != tiendaActual.IdTienda)
            {
                return NotFound();
            }

            var rolInterno = await ObtenerRolInternoActualAsync(tiendaActual.IdTienda);

            if (rolInterno != "admin_tienda")
            {
                TempData["MensajeError"] = "Solo el admin_tienda puede editar la información de la tienda.";
                return RedirectToAction(nameof(Index));
            }

            NormalizarTienda(tienda);
            ValidarTienda(tienda, tiendaActual.IdTienda);

            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Revisa los datos de la tienda.";
                return View(tienda);
            }

            tiendaActual.Descripcion = tienda.Descripcion;
            tiendaActual.LogoUrl = tienda.LogoUrl;
            tiendaActual.VisiblePublico = tienda.VisiblePublico;
            tiendaActual.RazonSocial = tienda.RazonSocial;
            tiendaActual.NombreRepresentante = tienda.NombreRepresentante;
            tiendaActual.TelefonoContacto = tienda.TelefonoContacto;
            tiendaActual.CorreoContacto = tienda.CorreoContacto;
            tiendaActual.DireccionComercial = tienda.DireccionComercial;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "La información de la tienda se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}