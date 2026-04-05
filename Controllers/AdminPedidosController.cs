using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using ECOMMERCE_NEXOSOFT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(1)]
    public class AdminPedidosController : Controller
    {
        private readonly NexosoftDbContext _context;

        public AdminPedidosController(NexosoftDbContext context)
        {
            _context = context;
        }

        private void RegistrarMovimientoCancelacion(Pedido pedido, int idUsuario)
        {
            var detalles = _context.Detallepedidos
                .Where(d => d.IdPedido == pedido.IdPedido)
                .ToList();

            foreach (var detalle in detalles)
            {
                var stock = _context.Stocks.FirstOrDefault(s => s.IdProducto == detalle.IdProducto);

                if (stock != null)
                {
                    int stockAnterior = stock.StockActual;
                    stock.StockActual += detalle.Cantidad;
                    int stockNuevo = stock.StockActual;

                    var movimiento = new MovimientoInventario
                    {
                        IdProducto = detalle.IdProducto,
                        IdUsuario = idUsuario,
                        TipoMovimiento = "cancelacion",
                        Cantidad = detalle.Cantidad,
                        StockAnterior = stockAnterior,
                        StockNuevo = stockNuevo,
                        Motivo = $"Reversión automática por cancelación del pedido {pedido.CodPedido}.",
                        FechaMovimiento = DateTime.Now
                    };

                    _context.MovimientoInventarios.Add(movimiento);
                }
            }
        }

        public async Task<IActionResult> Index(string? estado, string? buscar)
        {
            var query = _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdTiendaNavigation)
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
                    (p.IdUsuarioNavigation != null && p.IdUsuarioNavigation.Nombre.ToLower().Contains(texto)) ||
                    (p.IdTiendaNavigation != null && p.IdTiendaNavigation.NombreTienda.ToLower().Contains(texto)));
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

            var pedido = await _context.Pedidos
                .Include(p => p.IdUsuarioNavigation)
                .Include(p => p.IdTiendaNavigation)
                .Include(p => p.Detallepedidos)
                    .ThenInclude(d => d.IdProductoNavigation)
                .Include(p => p.Ventum)
                    .ThenInclude(v => v!.Pago)
                .FirstOrDefaultAsync(p => p.IdPedido == id);

            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int idPedido, string accion)
        {
            var pedido = await _context.Pedidos
                .FirstOrDefaultAsync(p => p.IdPedido == idPedido);

            if (pedido == null)
            {
                TempData["MensajeError"] = "No se encontró el pedido.";
                return RedirectToAction("Index");
            }

            var estadoActual = (pedido.EstadoPedido ?? string.Empty).Trim().ToLower();
            var accionNormalizada = (accion ?? string.Empty).Trim().ToLower();

            string? nuevoEstado = null;

            if (estadoActual == "pendiente" && accionNormalizada == "cancelar")
            {
                nuevoEstado = "cancelado";
            }

            if (nuevoEstado == null)
            {
                TempData["MensajeError"] = "La acción seleccionada no es válida para el estado actual del pedido.";
                return RedirectToAction("Details", new { id = idPedido });
            }

            pedido.EstadoPedido = nuevoEstado;

            if (nuevoEstado == "cancelado")
            {
                var idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

                RegistrarMovimientoCancelacion(pedido, idUsuario);

                var venta = _context.Venta
                    .Include(v => v.Pago)
                    .FirstOrDefault(v => v.IdPedido == pedido.IdPedido);

                if (venta?.Pago != null)
                {
                    venta.Pago.EstadoPago = "reembolsado";
                    venta.Pago.Descripcion = "Pago revertido automáticamente por cancelación del pedido.";
                }
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "El estado del pedido se actualizó correctamente.";
            return RedirectToAction("Details", new { id = idPedido });
        }
    }
}