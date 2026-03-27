using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using ECOMMERCE_NEXOSOFT.Models;
using ECOMMERCE_NEXOSOFT.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(1, 2, 3)]
    public class PerfilController : Controller
    {
        private readonly NexosoftDbContext _context;

        public PerfilController(NexosoftDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario.Value);

            if (usuario == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario.Value);

            var vm = new PerfilViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Telefono = usuario.Telefono ?? string.Empty,
                CorreoElectronico = usuario.CorreoElectronico,
                IdRol = usuario.IdRol,
                Direccion1 = cliente?.Direccion1,
                Direccion2 = cliente?.Direccion2,
                Direccion3 = cliente?.Direccion3
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PerfilViewModel model)
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null || idUsuario.Value != model.IdUsuario)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Nombre = model.Nombre.Trim();
            usuario.Apellido = model.Apellido.Trim();
            usuario.Telefono = model.Telefono.Trim();
            usuario.CorreoElectronico = model.CorreoElectronico.Trim();

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.IdUsuario == model.IdUsuario);

            if (cliente == null)
            {
                var ultimoCodCliente = await _context.Clientes
                    .OrderByDescending(c => c.CodCliente)
                    .Select(c => (int?)c.CodCliente)
                    .FirstOrDefaultAsync();

                int nuevoCodCliente = (ultimoCodCliente ?? 2000) + 1;

                cliente = new Cliente
                {
                    CodCliente = nuevoCodCliente,
                    IdUsuario = model.IdUsuario,
                    FechaRegistroCliente = DateTime.Now,
                    EstadoCliente = "activo"
                };

                _context.Clientes.Add(cliente);
            }

            cliente.Direccion1 = string.IsNullOrWhiteSpace(model.Direccion1) ? null : model.Direccion1.Trim();
            cliente.Direccion2 = string.IsNullOrWhiteSpace(model.Direccion2) ? null : model.Direccion2.Trim();
            cliente.Direccion3 = string.IsNullOrWhiteSpace(model.Direccion3) ? null : model.Direccion3.Trim();

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Usuario", usuario.Nombre);
            HttpContext.Session.SetString("Correo", usuario.CorreoElectronico);

            TempData["MensajeExito"] = "Perfil actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}