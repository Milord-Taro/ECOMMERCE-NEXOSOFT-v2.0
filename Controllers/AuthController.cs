using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Models;
using ECOMMERCE_NEXOSOFT.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    public class AuthController : Controller
    {
        private readonly NexosoftDbContext _context;

        public AuthController(NexosoftDbContext context)
        {
            _context = context;
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
                var errores = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var e in errores)
                {
                    Console.WriteLine(e.ErrorMessage);
                }

                return View(model);
            }

            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.CorreoElectronico == model.CorreoElectronico);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View(model);
            }

            bool passwordValida = false;

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
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View(model);
            }

            // Guardar sesión
            HttpContext.Session.SetString("Usuario", usuario.Nombre);
            HttpContext.Session.SetString("Correo", usuario.CorreoElectronico);
            HttpContext.Session.SetInt32("Rol", usuario.IdRol);
            HttpContext.Session.SetInt32("IdUsuario", usuario.IdUsuario);

            // Redirección por rol
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

        // ================= REGISTER =================

        // 🔹 GET (mostrar vista)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // 🔹 POST (procesar registro)
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 🔹 NORMALIZAR
            model.CorreoElectronico = model.CorreoElectronico.Trim().ToLower();
            model.Nombre = model.Nombre.Trim();
            model.Apellido = model.Apellido.Trim();

            // ❌ Correo no inicia con número
            if (char.IsDigit(model.CorreoElectronico[0]))
            {
                ModelState.AddModelError("CorreoElectronico", "El correo no puede iniciar con número");
                return View(model);
            }

            // ❌ Correo inválido tipo gmi.com
            if (model.CorreoElectronico.Contains("@gmi.com"))
            {
                ModelState.AddModelError("CorreoElectronico", "Dominio de correo inválido");
                return View(model);
            }

            // ❌ Validar teléfono colombiano
            if (!model.Telefono.StartsWith("3") || model.Telefono.Length != 10)
            {
                ModelState.AddModelError("Telefono", "Teléfono inválido");
                return View(model);
            }

            // ❌ Validar documento
            if (model.NumeroIdentificacion.Length != 10)
            {
                ModelState.AddModelError("NumeroIdentificacion", "Debe tener 10 dígitos");
                return View(model);
            }

            // ❌ Verificar si ya existe
            var existe = _context.Usuarios
                .Any(u => u.CorreoElectronico == model.CorreoElectronico);

            if (existe)
            {
                ModelState.AddModelError("CorreoElectronico", "El correo ya está registrado");
                return View(model);
            }

            // 🔐 HASH
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Contrasena);

            // 🔹 Código único
            var random = new Random();
            int codigo;
            do
            {
                codigo = random.Next(1000, 9999);
            } while (_context.Usuarios.Any(u => u.CodUsuario == codigo));

            // 🔹 Crear usuario
            Usuario nuevoUsuario = new Usuario
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
                FechaRegistro = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            _context.Usuarios.Add(nuevoUsuario);
            _context.SaveChanges();

            TempData["Success"] = "Registro exitoso";

            return RedirectToAction("Login");
        }
    }
}