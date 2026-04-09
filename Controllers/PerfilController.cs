using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using ECOMMERCE_NEXOSOFT.Helpers;
using ECOMMERCE_NEXOSOFT.Models;
using ECOMMERCE_NEXOSOFT.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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

        private bool EsTipoIdentificacionValido(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return false;

            return ValidationRules.ValidIdentificationTypes.Contains(tipo.Trim().ToLowerInvariant());
        }

        private bool EsNumeroIdentificacionValido(string? tipo, string? numero)
        {
            if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(numero))
                return false;

            tipo = tipo.Trim().ToLowerInvariant();
            numero = InputNormalizer.NormalizeIdentificationNumber(numero);

            if (tipo == "pasaporte")
                return Regex.IsMatch(numero, ValidationRules.PassportPattern);

            return Regex.IsMatch(numero, ValidationRules.NumericIdentificationPattern);
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

            var esCuentaInternaTienda = await _context.MiembroTiendas
                .AnyAsync(m => m.IdUsuario == usuario.IdUsuario);

            var vm = new PerfilViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Telefono = usuario.Telefono ?? string.Empty,
                CorreoElectronico = usuario.CorreoElectronico,
                IdRol = usuario.IdRol,
                TipoIdentificacion = usuario.TipoIdentificacion,
                NumeroIdentificacion = usuario.NumeroIdentificacion,
                EsCuentaInternaTienda = esCuentaInternaTienda,
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

            model.Nombre = InputNormalizer.NormalizeText(model.Nombre);
            model.Apellido = InputNormalizer.NormalizeText(model.Apellido);
            model.Telefono = InputNormalizer.NormalizePhone(model.Telefono);
            model.CorreoElectronico = InputNormalizer.NormalizeEmail(model.CorreoElectronico);
            model.TipoIdentificacion = InputNormalizer.NormalizeText(model.TipoIdentificacion).ToLowerInvariant();
            model.NumeroIdentificacion = InputNormalizer.NormalizeIdentificationNumber(model.NumeroIdentificacion);
            model.Direccion1 = string.IsNullOrWhiteSpace(model.Direccion1) ? null : InputNormalizer.NormalizeAddress(model.Direccion1);
            model.Direccion2 = string.IsNullOrWhiteSpace(model.Direccion2) ? null : InputNormalizer.NormalizeAddress(model.Direccion2);
            model.Direccion3 = string.IsNullOrWhiteSpace(model.Direccion3) ? null : InputNormalizer.NormalizeAddress(model.Direccion3);

            var esCuentaInternaTienda = await _context.MiembroTiendas
                .AnyAsync(m => m.IdUsuario == model.IdUsuario);

            model.EsCuentaInternaTienda = esCuentaInternaTienda;

            if (!EsTipoIdentificacionValido(model.TipoIdentificacion))
            {
                ModelState.AddModelError("TipoIdentificacion", "Debes seleccionar un tipo de identificación válido.");
            }

            if (!EsNumeroIdentificacionValido(model.TipoIdentificacion, model.NumeroIdentificacion))
            {
                ModelState.AddModelError("NumeroIdentificacion", "El número de identificación no cumple el formato permitido para el tipo seleccionado.");
            }

            var existeCorreo = await _context.Usuarios
                .AnyAsync(u => u.CorreoElectronico == model.CorreoElectronico && u.IdUsuario != model.IdUsuario);

            if (existeCorreo)
            {
                ModelState.AddModelError("CorreoElectronico", "El correo ya está registrado.");
            }

            var existeIdentificacion = await _context.Usuarios
                .AnyAsync(u => u.NumeroIdentificacion == model.NumeroIdentificacion && u.IdUsuario != model.IdUsuario);

            if (existeIdentificacion)
            {
                ModelState.AddModelError("NumeroIdentificacion", "El número de identificación ya está registrado.");
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

            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;
            usuario.Telefono = model.Telefono;
            usuario.CorreoElectronico = model.CorreoElectronico;
            usuario.TipoIdentificacion = model.TipoIdentificacion ?? usuario.TipoIdentificacion;
            usuario.NumeroIdentificacion = model.NumeroIdentificacion ?? usuario.NumeroIdentificacion;

            if (!esCuentaInternaTienda)
            {
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

                cliente.Direccion1 = model.Direccion1;
                cliente.Direccion2 = model.Direccion2;
                cliente.Direccion3 = model.Direccion3;
            }

            if (!string.IsNullOrWhiteSpace(model.NuevaContrasena))
            {
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(model.NuevaContrasena.Trim());
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Usuario", usuario.Nombre);
            HttpContext.Session.SetString("Correo", usuario.CorreoElectronico);

            TempData["MensajeExito"] = "Perfil actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}