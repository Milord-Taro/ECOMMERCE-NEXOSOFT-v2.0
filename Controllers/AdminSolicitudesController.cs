using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using ECOMMERCE_NEXOSOFT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(1)]
    public class AdminSolicitudesController : Controller
    {
        private readonly NexosoftDbContext _context;

        public AdminSolicitudesController(NexosoftDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudVendedors
                .Include(s => s.IdUsuarioNavigation)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var solicitud = await _context.SolicitudVendedors
                .Include(s => s.IdUsuarioNavigation)
                .FirstOrDefaultAsync(s => s.IdSolicitudVendedor == id);

            if (solicitud == null) return NotFound();

            return View(solicitud);
        }

        public async Task<IActionResult> Aprobar(int? id)
        {
            if (id == null) return NotFound();

            var solicitud = await _context.SolicitudVendedors
                .Include(s => s.IdUsuarioNavigation)
                .FirstOrDefaultAsync(s => s.IdSolicitudVendedor == id);

            if (solicitud == null) return NotFound();

            return View(solicitud);
        }

        [HttpPost, ActionName("Aprobar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprobarConfirmado(int id)
        {
            var solicitud = await _context.SolicitudVendedors
                .Include(s => s.IdUsuarioNavigation)
                .FirstOrDefaultAsync(s => s.IdSolicitudVendedor == id);

            if (solicitud == null) return NotFound();

            if (solicitud.EstadoSolicitud != "pendiente")
            {
                TempData["MensajeError"] = "Solo se pueden aprobar solicitudes pendientes.";
                return RedirectToAction(nameof(Index));
            }

            var yaEsVendedor = await _context.Vendedors.AnyAsync(v => v.IdUsuario == solicitud.IdUsuario);
            if (yaEsVendedor)
            {
                TempData["MensajeError"] = "El usuario ya está registrado como vendedor.";
                return RedirectToAction(nameof(Index));
            }

            var nombreTiendaExiste = await _context.Tiendas.AnyAsync(t => t.NombreTienda == solicitud.NombreTiendaSolicitada);
            if (nombreTiendaExiste)
            {
                TempData["MensajeError"] = "Ya existe una tienda con ese nombre.";
                return RedirectToAction(nameof(Index));
            }

            var ultimoCodVendedor = await _context.Vendedors
                .OrderByDescending(v => v.CodVendedor)
                .Select(v => (int?)v.CodVendedor)
                .FirstOrDefaultAsync();

            var ultimoCodTienda = await _context.Tiendas
                .OrderByDescending(t => t.CodTienda)
                .Select(t => (int?)t.CodTienda)
                .FirstOrDefaultAsync();

            int nuevoCodVendedor = (ultimoCodVendedor ?? 5000) + 1;
            int nuevoCodTienda = (ultimoCodTienda ?? 7000) + 1;

            var vendedor = new Vendedor
            {
                CodVendedor = nuevoCodVendedor,
                IdUsuario = solicitud.IdUsuario,
                EstadoVendedor = "activo",
                FechaRegistro = DateTime.Now
            };

            _context.Vendedors.Add(vendedor);
            await _context.SaveChangesAsync();

            var tienda = new Tienda
            {
                CodTienda = nuevoCodTienda,
                IdVendedor = vendedor.IdVendedor,
                NombreTienda = solicitud.NombreTiendaSolicitada,
                Descripcion = solicitud.DescripcionTienda,
                VisiblePublico = true,
                FechaRegistro = DateTime.Now
            };

            _context.Tiendas.Add(tienda);

            var usuario = solicitud.IdUsuarioNavigation;
            usuario.IdRol = 3;

            solicitud.EstadoSolicitud = "aprobada";
            solicitud.FechaRespuesta = DateTime.Now;
            solicitud.ObservacionAdmin = "Solicitud aprobada.";

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Solicitud aprobada correctamente. Se creó el vendedor y la tienda.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Rechazar(int? id)
        {
            if (id == null) return NotFound();

            var solicitud = await _context.SolicitudVendedors
                .Include(s => s.IdUsuarioNavigation)
                .FirstOrDefaultAsync(s => s.IdSolicitudVendedor == id);

            if (solicitud == null) return NotFound();

            return View(solicitud);
        }

        [HttpPost, ActionName("Rechazar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RechazarConfirmado(int id, string? observacionAdmin)
        {
            var solicitud = await _context.SolicitudVendedors.FirstOrDefaultAsync(s => s.IdSolicitudVendedor == id);

            if (solicitud == null) return NotFound();

            if (solicitud.EstadoSolicitud != "pendiente")
            {
                TempData["MensajeError"] = "Solo se pueden rechazar solicitudes pendientes.";
                return RedirectToAction(nameof(Index));
            }

            solicitud.EstadoSolicitud = "rechazada";
            solicitud.FechaRespuesta = DateTime.Now;
            solicitud.ObservacionAdmin = string.IsNullOrWhiteSpace(observacionAdmin)
                ? "Solicitud rechazada por el administrador."
                : observacionAdmin.Trim();

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Solicitud rechazada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}