using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(3)]
    public class VendedorPedidosController : Controller
    {
        private readonly NexosoftDbContext _context;

        public VendedorPedidosController(NexosoftDbContext context)
        {
            _context = context;
        }

        private int? ObtenerIdTiendaVendedorLogueado()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return null;
            }

            var idTienda = _context.Tiendas
                .Include(t => t.IdVendedorNavigation)
                .Where(t => t.IdVendedorNavigation.IdUsuario == idUsuario.Value)
                .Select(t => (int?)t.IdTienda)
                .FirstOrDefault();

            return idTienda;
        }

        public async Task<IActionResult> Index(string? estado, string? buscar)
        {
            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada para consultar pedidos.";
                return RedirectToAction("Index", "Vendedor");
            }

            var query = _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdTiendaNavigation)
                .Where(p => p.IdTienda == idTienda.Value)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estado))
            {
                query = query.Where(p => p.EstadoPedido == estado);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim().ToLower();

                query = query.Where(p =>
                    p.CodPedido.ToString().Contains(texto) ||
                    (p.IdUsuarioNavigation != null &&
                     ((p.IdUsuarioNavigation.Nombre + " " + p.IdUsuarioNavigation.Apellido).ToLower().Contains(texto) ||
                      p.IdUsuarioNavigation.CorreoElectronico.ToLower().Contains(texto))));
            }

            var pedidos = await query
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            ViewBag.EstadoSeleccionado = estado;
            ViewBag.Busqueda = buscar;

            return View(pedidos);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada para consultar pedidos.";
                return RedirectToAction("Index", "Vendedor");
            }

            var pedido = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdTiendaNavigation)
                .Include(p => p.Detallepedidos)
                    .ThenInclude(d => d.IdProductoNavigation)
                .FirstOrDefaultAsync(p => p.IdPedido == id && p.IdTienda == idTienda.Value);

            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }
    }
}