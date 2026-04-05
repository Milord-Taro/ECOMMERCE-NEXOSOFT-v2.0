using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Helpers;
using ECOMMERCE_NEXOSOFT.Models;
using ECOMMERCE_NEXOSOFT.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    public class AuthController : Controller
    {
        private readonly NexosoftDbContext _context;

        public AuthController(NexosoftDbContext context)
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

        // ================= LOGIN =================
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CorreoElectronico = InputNormalizer.NormalizeEmail(model.CorreoElectronico);

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.CorreoElectronico == model.CorreoElectronico);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View(model);
            }

            bool passwordValida;

            try
            {
                passwordValida = BCrypt.Net.BCrypt.Verify(model.Contrasena, usuario.Contrasena);
            }
            catch
            {
                passwordValida = false;
            }

            if (!passwordValida)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View(model);
            }

            HttpContext.Session.SetString("Usuario", usuario.Nombre);
            HttpContext.Session.SetString("Correo", usuario.CorreoElectronico);
            HttpContext.Session.SetInt32("Rol", usuario.IdRol);
            HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);

            return usuario.IdRol switch
            {
                1 => RedirectToAction("Index", "Admin"),
                2 => RedirectToAction("Index", "Home"),
                3 => RedirectToAction("Index", "Vendedor"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // ================= LOGOUT =================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        // ================= FORGOTPASSWORD =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CorreoElectronico = InputNormalizer.NormalizeEmail(model.CorreoElectronico);

            var usuario = _context.Usuarios.FirstOrDefault(u => u.CorreoElectronico == model.CorreoElectronico);

            if (usuario == null)
            {
                ViewBag.Error = "No existe una cuenta con ese correo.";
                return View(model);
            }

            return RedirectToAction("ResetPassword", new { correo = model.CorreoElectronico });
        }

        public IActionResult ResetPassword(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel
            {
                CorreoElectronico = InputNormalizer.NormalizeEmail(correo)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.CorreoElectronico = InputNormalizer.NormalizeEmail(model.CorreoElectronico);

            var usuario = _context.Usuarios.FirstOrDefault(u => u.CorreoElectronico == model.CorreoElectronico);

            if (usuario == null)
            {
                ViewBag.Error = "No existe una cuenta con ese correo.";
                return View(model);
            }

            usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(model.NuevaContrasena.Trim());

            _context.Update(usuario);
            _context.SaveChanges();

            TempData["MensajeExito"] = "La contraseña fue actualizada correctamente.";

            return RedirectToAction("Login");
        }

        // ================= REGISTER =================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Nombre = InputNormalizer.NormalizeText(model.Nombre);
            model.Apellido = InputNormalizer.NormalizeText(model.Apellido);
            model.TipoIdentificacion = InputNormalizer.NormalizeText(model.TipoIdentificacion).ToLowerInvariant();
            model.NumeroIdentificacion = InputNormalizer.NormalizeIdentificationNumber(model.NumeroIdentificacion);
            model.Telefono = InputNormalizer.NormalizePhone(model.Telefono);
            model.CorreoElectronico = InputNormalizer.NormalizeEmail(model.CorreoElectronico);

            if (!EsTipoIdentificacionValido(model.TipoIdentificacion))
            {
                ModelState.AddModelError("TipoIdentificacion", "Debes seleccionar un tipo de identificación válido.");
            }

            if (!EsNumeroIdentificacionValido(model.TipoIdentificacion, model.NumeroIdentificacion))
            {
                ModelState.AddModelError("NumeroIdentificacion", "El número de identificación no cumple el formato permitido para el tipo seleccionado.");
            }

            if (_context.Usuarios.Any(u => u.CorreoElectronico == model.CorreoElectronico))
            {
                ModelState.AddModelError("CorreoElectronico", "El correo ya está registrado.");
            }

            if (_context.Usuarios.Any(u => u.NumeroIdentificacion == model.NumeroIdentificacion))
            {
                ModelState.AddModelError("NumeroIdentificacion", "El número de identificación ya está registrado.");
            }

            if (!ModelState.IsValid)
                return View(model);

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Contrasena.Trim());

            var random = new Random();
            int codigo;
            do
            {
                codigo = random.Next(1000, 9999);
            } while (_context.Usuarios.Any(u => u.CodUsuario == codigo));

            var nuevoUsuario = new Usuario
            {
                CodUsuario = codigo,
                IdRol = 2,
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                TipoIdentificacion = model.TipoIdentificacion,
                NumeroIdentificacion = model.NumeroIdentificacion,
                Telefono = model.Telefono,
                CorreoElectronico = model.CorreoElectronico,
                Contrasena = passwordHash,
                FechaRegistro = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            TempData["Success"] = "Registro exitoso.";

            return RedirectToAction("Login");
        }
    }
}