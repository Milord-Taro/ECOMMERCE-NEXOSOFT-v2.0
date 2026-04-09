using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(2, 3)]
    public class MisPedidosController : Controller
    {
        private readonly NexosoftDbContext _context;

        public MisPedidosController(NexosoftDbContext context)
        {
            _context = context;
        }
        private bool EsCuentaInternaSinPermisoCompra(int idUsuario)
        {
            return _context.MiembroTiendas
                .Include(m => m.IdRolTiendaNavigation)
                .Any(m => m.IdUsuario == idUsuario &&
                          m.IdRolTiendaNavigation.NombreRol != "admin_tienda");
        }

        private IActionResult RedirigirCompraBloqueada()
        {
            TempData["MensajeError"] = "Las cuentas internas de tienda no tienen acceso al módulo de compras del cliente.";
            return RedirectToAction("Index", "Vendedor");
        }

        public async Task<IActionResult> Index()
        {

            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (EsCuentaInternaSinPermisoCompra(idUsuario.Value))
            {
                return RedirigirCompraBloqueada();
            }

            var pedidos = await _context.Pedidos
                .Include(p => p.IdTiendaNavigation)
                .Where(p => p.IdUsuario == idUsuario.Value)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(pedidos);
        }

        public async Task<IActionResult> Detalle(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (EsCuentaInternaSinPermisoCompra(idUsuario.Value))
            {
                return RedirigirCompraBloqueada();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdTiendaNavigation)
                .Include(p => p.Detallepedidos)
                    .ThenInclude(d => d.IdProductoNavigation)
                .Include(p => p.Ventum)
                    .ThenInclude(v => v!.Pago)
                .FirstOrDefaultAsync(p => p.IdPedido == id && p.IdUsuario == idUsuario.Value);

            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        public async Task<IActionResult> Comprobante(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (EsCuentaInternaSinPermisoCompra(idUsuario.Value))
            {
                return RedirigirCompraBloqueada();
            }

            var pedido = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdTiendaNavigation)
                .Include(p => p.Detallepedidos)
                    .ThenInclude(d => d.IdProductoNavigation)
                .Include(p => p.Ventum)
                    .ThenInclude(v => v!.Pago)
                .FirstOrDefaultAsync(p => p.IdPedido == id && p.IdUsuario == idUsuario.Value);

            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }
    }
}