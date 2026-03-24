using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
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

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario.Value);

            if (usuario == null)
            {
                return NotFound();
            }

            var vm = new PerfilViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Telefono = usuario.Telefono ?? string.Empty,
                CorreoElectronico = usuario.CorreoElectronico,
                IdRol = usuario.IdRol
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

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == model.IdUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Telefono = model.Telefono;
            usuario.CorreoElectronico = model.CorreoElectronico;

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Usuario", usuario.Nombre);
            HttpContext.Session.SetString("Correo", usuario.CorreoElectronico);

            TempData["MensajeExito"] = "Perfil actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}