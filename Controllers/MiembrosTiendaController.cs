using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using ECOMMERCE_NEXOSOFT.Models;
using ECOMMERCE_NEXOSOFT.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(3)]
    public class MiembrosTiendaController : Controller
    {
        private readonly NexosoftDbContext _context;

        public MiembrosTiendaController(NexosoftDbContext context)
        {
            _context = context;
        }

        private async Task<Tienda?> ObtenerTiendaActualAsync()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return null;
            }

            var tiendaPorMiembro = await _context.MiembroTiendas
                .Include(m => m.IdTiendaNavigation)
                    .ThenInclude(t => t.IdVendedorNavigation)
                .Where(m => m.IdUsuario == idUsuario.Value)
                .Select(m => m.IdTiendaNavigation)
                .FirstOrDefaultAsync();

            if (tiendaPorMiembro != null)
            {
                return tiendaPorMiembro;
            }

            return await _context.Tiendas
                .Include(t => t.IdVendedorNavigation)
                .FirstOrDefaultAsync(t => t.IdVendedorNavigation.IdUsuario == idUsuario.Value);
        }

        private async Task AsegurarFundadorComoAdminTiendaAsync(Tienda tienda)
        {
            if (tienda.IdVendedorNavigation == null)
            {
                tienda = await _context.Tiendas
                    .Include(t => t.IdVendedorNavigation)
                    .FirstOrDefaultAsync(t => t.IdTienda == tienda.IdTienda)
                    ?? tienda;
            }

            if (tienda.IdVendedorNavigation == null)
            {
                return;
            }

            var idUsuarioFundador = tienda.IdVendedorNavigation.IdUsuario;

            var yaExiste = await _context.MiembroTiendas
                .AnyAsync(m => m.IdUsuario == idUsuarioFundador && m.IdTienda == tienda.IdTienda);

            if (yaExiste)
            {
                return;
            }

            var rolAdminTienda = await _context.RolTiendas
                .FirstOrDefaultAsync(r => r.NombreRol == "admin_tienda");

            if (rolAdminTienda == null)
            {
                return;
            }

            var miembroFundador = new MiembroTienda
            {
                IdUsuario = idUsuarioFundador,
                IdTienda = tienda.IdTienda,
                IdRolTienda = rolAdminTienda.IdRolTienda,
                FechaIngreso = DateTime.Now
            };

            _context.MiembroTiendas.Add(miembroFundador);
            await _context.SaveChangesAsync();
        }

        private async Task<bool> UsuarioActualEsAdminTiendaAsync(Tienda tienda)
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return false;
            }

            var esAdminTienda = await _context.MiembroTiendas
                .Include(m => m.IdRolTiendaNavigation)
                .AnyAsync(m =>
                    m.IdUsuario == idUsuario.Value &&
                    m.IdTienda == tienda.IdTienda &&
                    m.IdRolTiendaNavigation.NombreRol == "admin_tienda");

            return esAdminTienda;
        }

        private async Task CargarRolesInternosAsync(int? rolSeleccionado = null)
        {
            var roles = await _context.RolTiendas
                .Where(r => r.NombreRol != "admin_tienda")
                .OrderBy(r => r.NombreRol)
                .ToListAsync();

            var items = roles.Select(r => new SelectListItem
            {
                Value = r.IdRolTienda.ToString(),
                Text = r.NombreRol == "gestor_tienda"
                    ? "Gestor tienda"
                    : r.NombreRol == "vendedor_tienda"
                        ? "Vendedor tienda"
                        : r.NombreRol
            }).ToList();

            ViewBag.RolesInternos = new SelectList(items, "Value", "Text", rolSeleccionado?.ToString());
        }

        private async Task<MiembroTienda?> ObtenerMiembroEditableAsync(int idMiembroTienda, int idTienda)
        {
            return await _context.MiembroTiendas
                .Include(m => m.IdUsuarioNavigation)
                .Include(m => m.IdRolTiendaNavigation)
                .FirstOrDefaultAsync(m => m.IdMiembroTienda == idMiembroTienda && m.IdTienda == idTienda);
        }

        public async Task<IActionResult> Index()
        {
            var tienda = await ObtenerTiendaActualAsync();

            if (tienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction("Index", "Vendedor");
            }

            await AsegurarFundadorComoAdminTiendaAsync(tienda);

            var miembros = await _context.MiembroTiendas
                .Include(m => m.IdUsuarioNavigation)
                .Include(m => m.IdRolTiendaNavigation)
                .Where(m => m.IdTienda == tienda.IdTienda)
                .OrderBy(m => m.IdRolTiendaNavigation.NombreRol == "admin_tienda" ? 0 : 1)
                .ThenBy(m => m.FechaIngreso)
                .ToListAsync();

            ViewBag.NombreTienda = tienda.NombreTienda;
            ViewBag.IdTienda = tienda.IdTienda;
            ViewBag.EsAdminTienda = await UsuarioActualEsAdminTiendaAsync(tienda);

            return View(miembros);
        }

        public async Task<IActionResult> Create()
        {
            var tienda = await ObtenerTiendaActualAsync();

            if (tienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction("Index", "Vendedor");
            }

            await AsegurarFundadorComoAdminTiendaAsync(tienda);

            if (!await UsuarioActualEsAdminTiendaAsync(tienda))
            {
                TempData["MensajeError"] = "Solo el Admin Tienda puede crear miembros internos.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.NombreTienda = tienda.NombreTienda;
            await CargarRolesInternosAsync();

            return View(new MiembroTiendaCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MiembroTiendaCreateViewModel model)
        {
            var tienda = await ObtenerTiendaActualAsync();

            if (tienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction("Index", "Vendedor");
            }

            await AsegurarFundadorComoAdminTiendaAsync(tienda);

            if (!await UsuarioActualEsAdminTiendaAsync(tienda))
            {
                TempData["MensajeError"] = "Solo el Admin Tienda puede crear miembros internos.";
                return RedirectToAction(nameof(Index));
            }

            if (await _context.Usuarios.AnyAsync(u => u.CorreoElectronico == model.CorreoElectronico.Trim()))
            {
                ModelState.AddModelError("CorreoElectronico", "Ya existe un usuario con ese correo.");
            }

            if (await _context.Usuarios.AnyAsync(u => u.NumeroIdentificacion == model.NumeroIdentificacion.Trim()))
            {
                ModelState.AddModelError("NumeroIdentificacion", "Ya existe un usuario con ese número de identificación.");
            }

            var rolInterno = await _context.RolTiendas.FirstOrDefaultAsync(r => r.IdRolTienda == model.IdRolTienda);

            if (rolInterno != null && rolInterno.NombreRol == "admin_tienda")
            {
                ModelState.AddModelError("IdRolTienda", "No está permitido crear otro Admin Tienda desde este módulo.");
            }

            if (rolInterno == null)
            {
                ModelState.AddModelError("IdRolTienda", "Debes seleccionar un rol interno válido.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.NombreTienda = tienda.NombreTienda;
                await CargarRolesInternosAsync(model.IdRolTienda);
                return View(model);
            }

            var ultimoCodUsuario = await _context.Usuarios
                .OrderByDescending(u => u.CodUsuario)
                .Select(u => (int?)u.CodUsuario)
                .FirstOrDefaultAsync();

            int nuevoCodUsuario = (ultimoCodUsuario ?? 1000) + 1;

            var nuevoUsuario = new Usuario
            {
                CodUsuario = nuevoCodUsuario,
                IdRol = 3,
                Nombre = model.Nombre.Trim(),
                Apellido = model.Apellido.Trim(),
                TipoIdentificacion = model.TipoIdentificacion.Trim(),
                NumeroIdentificacion = model.NumeroIdentificacion.Trim(),
                Telefono = model.Telefono.Trim(),
                CorreoElectronico = model.CorreoElectronico.Trim(),
                Contrasena = BCrypt.Net.BCrypt.HashPassword(model.Contrasena),
                FechaRegistro = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            var miembro = new MiembroTienda
            {
                IdUsuario = nuevoUsuario.IdUsuario,
                IdTienda = tienda.IdTienda,
                IdRolTienda = model.IdRolTienda,
                FechaIngreso = DateTime.Now
            };

            _context.MiembroTiendas.Add(miembro);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Miembro interno creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tienda = await ObtenerTiendaActualAsync();

            if (tienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction("Index", "Vendedor");
            }

            await AsegurarFundadorComoAdminTiendaAsync(tienda);

            if (!await UsuarioActualEsAdminTiendaAsync(tienda))
            {
                TempData["MensajeError"] = "Solo el Admin Tienda puede editar miembros internos.";
                return RedirectToAction(nameof(Index));
            }

            var miembro = await ObtenerMiembroEditableAsync(id.Value, tienda.IdTienda);

            if (miembro == null)
            {
                return NotFound();
            }

            ViewBag.NombreTienda = tienda.NombreTienda;
            await CargarRolesInternosAsync(miembro.IdRolTienda);

            var model = new ECOMMERCE_NEXOSOFT.ViewModels.MiembroTiendaEditViewModel
            {
                IdMiembroTienda = miembro.IdMiembroTienda,
                IdUsuario = miembro.IdUsuario,
                Nombre = miembro.IdUsuarioNavigation.Nombre,
                Apellido = miembro.IdUsuarioNavigation.Apellido,
                TipoIdentificacion = miembro.IdUsuarioNavigation.TipoIdentificacion,
                NumeroIdentificacion = miembro.IdUsuarioNavigation.NumeroIdentificacion,
                Telefono = miembro.IdUsuarioNavigation.Telefono ?? string.Empty,
                CorreoElectronico = miembro.IdUsuarioNavigation.CorreoElectronico,
                IdRolTienda = miembro.IdRolTienda
            };

            ViewBag.EsAdminRolFijo = miembro.IdRolTiendaNavigation?.NombreRol == "admin_tienda";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ECOMMERCE_NEXOSOFT.ViewModels.MiembroTiendaEditViewModel model)
        {
            var tienda = await ObtenerTiendaActualAsync();

            if (tienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction("Index", "Vendedor");
            }

            await AsegurarFundadorComoAdminTiendaAsync(tienda);

            if (!await UsuarioActualEsAdminTiendaAsync(tienda))
            {
                TempData["MensajeError"] = "Solo el Admin Tienda puede editar miembros internos.";
                return RedirectToAction(nameof(Index));
            }

            var miembro = await ObtenerMiembroEditableAsync(model.IdMiembroTienda, tienda.IdTienda);

            if (miembro == null)
            {
                return NotFound();
            }

            if (miembro.IdRolTiendaNavigation?.NombreRol == "admin_tienda" &&
                model.IdRolTienda != miembro.IdRolTienda)
            {
                ModelState.AddModelError("IdRolTienda", "El rol interno de admin_tienda no puede modificarse desde este módulo.");
            }

            var idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuarioSesion != null &&
                miembro.IdUsuario == idUsuarioSesion.Value &&
                miembro.IdRolTiendaNavigation?.NombreRol == "admin_tienda" &&
                miembro.IdRolTienda != model.IdRolTienda)
            {
                ModelState.AddModelError("IdRolTienda", "No puedes cambiar tu propio rol interno de Admin Tienda.");
            }

            var rolInterno = await _context.RolTiendas.FirstOrDefaultAsync(r => r.IdRolTienda == model.IdRolTienda);

            if (rolInterno == null)
            {
                ModelState.AddModelError("IdRolTienda", "Debes seleccionar un rol interno válido.");
            }

            if (rolInterno != null && rolInterno.NombreRol == "admin_tienda")
            {
                ModelState.AddModelError("IdRolTienda", "No está permitido asignar Admin Tienda desde este módulo.");
            }

            var correoNormalizado = model.CorreoElectronico.Trim();
            var identificacionNormalizada = model.NumeroIdentificacion.Trim();

            var existeCorreo = await _context.Usuarios
                .AnyAsync(u => u.CorreoElectronico == correoNormalizado && u.IdUsuario != model.IdUsuario);

            if (existeCorreo)
            {
                ModelState.AddModelError("CorreoElectronico", "Ya existe un usuario con ese correo.");
            }

            var existeIdentificacion = await _context.Usuarios
                .AnyAsync(u => u.NumeroIdentificacion == identificacionNormalizada && u.IdUsuario != model.IdUsuario);

            if (existeIdentificacion)
            {
                ModelState.AddModelError("NumeroIdentificacion", "Ya existe un usuario con ese número de identificación.");
            }

            var quiereCambiarContrasena =
               !string.IsNullOrWhiteSpace(model.NuevaContrasena) ||
               !string.IsNullOrWhiteSpace(model.ConfirmarNuevaContrasena);

            if (quiereCambiarContrasena)
            {
                if (string.IsNullOrWhiteSpace(model.NuevaContrasena))
                {
                    ModelState.AddModelError("NuevaContrasena", "Debes ingresar una nueva contraseña.");
                }

                if (string.IsNullOrWhiteSpace(model.ConfirmarNuevaContrasena))
                {
                    ModelState.AddModelError("ConfirmarNuevaContrasena", "Debes confirmar la nueva contraseña.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.NombreTienda = tienda.NombreTienda;
                await CargarRolesInternosAsync(model.IdRolTienda);
                ViewBag.EsAdminRolFijo = miembro.IdRolTiendaNavigation?.NombreRol == "admin_tienda";
                return View(model);
            }

            miembro.IdRolTienda = model.IdRolTienda;

            miembro.IdUsuarioNavigation.Nombre = model.Nombre.Trim();
            miembro.IdUsuarioNavigation.Apellido = model.Apellido.Trim();
            miembro.IdUsuarioNavigation.TipoIdentificacion = model.TipoIdentificacion.Trim();
            miembro.IdUsuarioNavigation.NumeroIdentificacion = identificacionNormalizada;
            miembro.IdUsuarioNavigation.Telefono = model.Telefono.Trim();
            miembro.IdUsuarioNavigation.CorreoElectronico = correoNormalizado;

            if (!string.IsNullOrWhiteSpace(model.NuevaContrasena))
            {
                miembro.IdUsuarioNavigation.Contrasena = BCrypt.Net.BCrypt.HashPassword(model.NuevaContrasena.Trim());
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Miembro interno actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}