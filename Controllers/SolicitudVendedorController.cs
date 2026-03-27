using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Models;
using ECOMMERCE_NEXOSOFT.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    public class SolicitudVendedorController : Controller
    {
        private readonly NexosoftDbContext _context;

        public SolicitudVendedorController(NexosoftDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolicitudVendedorViewModel model)
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "Revisa los campos obligatorios de la solicitud.";
                return View(model);
            }

            var yaEsVendedor = await _context.Vendedors.AnyAsync(v => v.IdUsuario == idUsuario.Value);
            if (yaEsVendedor)
            {
                TempData["MensajeError"] = "Tu cuenta ya está registrada como vendedor.";
                return View(model);
            }

            var tienePendiente = await _context.SolicitudVendedors.AnyAsync(s =>
                s.IdUsuario == idUsuario.Value &&
                s.EstadoSolicitud == "pendiente");

            if (tienePendiente)
            {
                TempData["MensajeError"] = "Ya tienes una solicitud pendiente de revisión.";
                return View(model);
            }

            var nombreTiendaExiste = await _context.Tiendas.AnyAsync(t =>
                t.NombreTienda.ToLower() == model.NombreTiendaSolicitada.ToLower());

            if (nombreTiendaExiste)
            {
                TempData["MensajeError"] = "Ya existe una tienda con ese nombre. Elige otro nombre.";
                return View(model);
            }

            var random = new Random();

            var solicitud = new SolicitudVendedor
            {
                CodSolicitudVendedor = random.Next(100000, 999999),
                IdUsuario = idUsuario.Value,
                NombreTiendaSolicitada = model.NombreTiendaSolicitada.Trim(),
                DescripcionTienda = string.IsNullOrWhiteSpace(model.DescripcionTienda)
                    ? null
                    : model.DescripcionTienda.Trim(),
                EstadoSolicitud = "pendiente",
                FechaSolicitud = DateTime.Now
            };

            _context.SolicitudVendedors.Add(solicitud);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Tu solicitud fue enviada correctamente. Quedará pendiente de aprobación por el administrador.";
            return RedirectToAction(nameof(MisSolicitudes));
        }

        public async Task<IActionResult> MisSolicitudes()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var solicitudes = await _context.SolicitudVendedors
                .Where(s => s.IdUsuario == idUsuario.Value)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }
    }
}