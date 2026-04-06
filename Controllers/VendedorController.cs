using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(3)]
    public class VendedorController : Controller
    {
        private readonly NexosoftDbContext _context;

        public VendedorController(NexosoftDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return View();
            }

            var tienda = await _context.Tiendas
                .FirstOrDefaultAsync(t => t.IdTienda == idTienda.Value);

            ViewBag.NombreTienda = tienda?.NombreTienda ?? "Mi tienda";
            ViewBag.TiendaVisiblePublico = tienda?.VisiblePublico ?? false;

            ViewBag.TotalProductos = await _context.Productos.CountAsync(p => p.IdTienda == idTienda.Value);
            ViewBag.ProductosPublicados = await _context.Productos.CountAsync(p => p.IdTienda == idTienda.Value && p.VisiblePublico);
            ViewBag.ProductosOcultos = await _context.Productos.CountAsync(p => p.IdTienda == idTienda.Value && !p.VisiblePublico);

            ViewBag.TotalStockBajo = await _context.Stocks.CountAsync(s =>
                s.IdProductoNavigation != null &&
                s.IdProductoNavigation.IdTienda == idTienda.Value &&
                s.StockActual <= s.StockMinimo);

            ViewBag.TotalPedidos = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value);
            ViewBag.PedidosPendientes = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value && p.EstadoPedido == "pendiente");
            ViewBag.PedidosEnPreparacion = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value && p.EstadoPedido == "en preparación");
            ViewBag.PedidosEnCamino = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value && p.EstadoPedido == "en camino");
            ViewBag.PedidosEntregados = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value && p.EstadoPedido == "entregado");

            ViewBag.TotalVentas = await _context.Venta.CountAsync(v => v.IdPedidoNavigation.IdTienda == idTienda.Value);
            ViewBag.TotalPagosAprobados = await _context.Pagos.CountAsync(p =>
                p.EstadoPago == "aprobado" &&
                p.IdVentaNavigation.IdPedidoNavigation.IdTienda == idTienda.Value &&
                p.IdVentaNavigation.IdPedidoNavigation.EstadoPedido != "cancelado");

            ViewBag.TotalRecaudado = await _context.Pagos
                .Where(p => p.EstadoPago == "aprobado" &&
                            p.IdVentaNavigation.IdPedidoNavigation.IdTienda == idTienda.Value &&
                            p.IdVentaNavigation.IdPedidoNavigation.EstadoPedido != "cancelado")
                .SumAsync(p => (decimal?)p.MontoPagado) ?? 0m;

            ViewBag.StockCritico = await _context.Stocks
                .Include(s => s.IdProductoNavigation)
                .Where(s => s.IdProductoNavigation != null && s.IdProductoNavigation.IdTienda == idTienda.Value)
                .OrderBy(s => s.StockActual)
                .Take(5)
                .ToListAsync();

            ViewBag.UltimosPedidos = await _context.Pedidos
                .Where(p => p.IdTienda == idTienda.Value)
                .OrderByDescending(p => p.FechaCreacion)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> Metricas()
        {
            var idTienda = ObtenerIdTiendaVendedorLogueado();

            if (idTienda == null)
            {
                TempData["MensajeError"] = "No tienes una tienda asociada.";
                return RedirectToAction(nameof(Index));
            }

            var tienda = await _context.Tiendas
                .FirstOrDefaultAsync(t => t.IdTienda == idTienda.Value);

            ViewBag.NombreTienda = tienda?.NombreTienda ?? "Mi tienda";
            ViewBag.TiendaVisiblePublico = tienda?.VisiblePublico ?? false;

            var totalProductos = await _context.Productos.CountAsync(p => p.IdTienda == idTienda.Value);
            var productosPublicados = await _context.Productos.CountAsync(p => p.IdTienda == idTienda.Value && p.VisiblePublico);
            var productosOcultos = await _context.Productos.CountAsync(p => p.IdTienda == idTienda.Value && !p.VisiblePublico);

            var totalStockBajo = await _context.Stocks.CountAsync(s =>
                s.IdProductoNavigation != null &&
                s.IdProductoNavigation.IdTienda == idTienda.Value &&
                s.StockActual <= s.StockMinimo);

            var totalPedidos = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value);
            var pedidosPendientes = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value && p.EstadoPedido == "pendiente");
            var pedidosEnPreparacion = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value && p.EstadoPedido == "en preparación");
            var pedidosEnCamino = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value && p.EstadoPedido == "en camino");
            var pedidosEntregados = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value && p.EstadoPedido == "entregado");
            var pedidosCancelados = await _context.Pedidos.CountAsync(p => p.IdTienda == idTienda.Value && p.EstadoPedido == "cancelado");

            var pedidosUltimos7Dias = await _context.Pedidos.CountAsync(p =>
                p.IdTienda == idTienda.Value &&
                p.FechaCreacion >= DateTime.Now.AddDays(-7));

            var pedidosUltimos30Dias = await _context.Pedidos.CountAsync(p =>
                p.IdTienda == idTienda.Value &&
                p.FechaCreacion >= DateTime.Now.AddDays(-30));

            var totalVentas = await _context.Venta.CountAsync(v => v.IdPedidoNavigation.IdTienda == idTienda.Value);

            var pagosAprobados = await _context.Pagos.CountAsync(p =>
                p.EstadoPago == "aprobado" &&
                p.IdVentaNavigation.IdPedidoNavigation.IdTienda == idTienda.Value &&
                p.IdVentaNavigation.IdPedidoNavigation.EstadoPedido != "cancelado");

            var pagosEnProceso = await _context.Pagos.CountAsync(p =>
                p.EstadoPago == "enproceso" &&
                p.IdVentaNavigation.IdPedidoNavigation.IdTienda == idTienda.Value);

            var pagosDesaprobados = await _context.Pagos.CountAsync(p =>
                p.EstadoPago == "desaprobado" &&
                p.IdVentaNavigation.IdPedidoNavigation.IdTienda == idTienda.Value);

            var totalRecaudado = await _context.Pagos
                .Where(p => p.EstadoPago == "aprobado" &&
                p.IdVentaNavigation.IdPedidoNavigation.IdTienda == idTienda.Value &&
                p.IdVentaNavigation.IdPedidoNavigation.EstadoPedido != "cancelado")
                .SumAsync(p => (decimal?)p.MontoPagado) ?? 0m;

            var topProductos = await _context.Detallepedidos
                .Where(d => d.IdPedidoNavigation.IdTienda == idTienda.Value)
                .GroupBy(d => d.IdProductoNavigation.NombreProducto)
                .Select(g => new
                {
                    NombreProducto = g.Key,
                    UnidadesVendidas = g.Sum(x => x.Cantidad),
                    TotalVendido = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.UnidadesVendidas)
                .Take(5)
                .ToListAsync();

            var productosStockCritico = await _context.Stocks
                .Include(s => s.IdProductoNavigation)
                .Where(s => s.IdProductoNavigation != null &&
                            s.IdProductoNavigation.IdTienda == idTienda.Value &&
                            s.StockActual <= s.StockMinimo)
                .OrderBy(s => s.StockActual)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalProductos = totalProductos;
            ViewBag.ProductosPublicados = productosPublicados;
            ViewBag.ProductosOcultos = productosOcultos;
            ViewBag.TotalStockBajo = totalStockBajo;

            ViewBag.TotalPedidos = totalPedidos;
            ViewBag.PedidosPendientes = pedidosPendientes;
            ViewBag.PedidosEnPreparacion = pedidosEnPreparacion;
            ViewBag.PedidosEnCamino = pedidosEnCamino;
            ViewBag.PedidosEntregados = pedidosEntregados;
            ViewBag.PedidosCancelados = pedidosCancelados;
            ViewBag.PedidosUltimos7Dias = pedidosUltimos7Dias;
            ViewBag.PedidosUltimos30Dias = pedidosUltimos30Dias;

            ViewBag.TotalVentas = totalVentas;
            ViewBag.PagosAprobados = pagosAprobados;
            ViewBag.PagosEnProceso = pagosEnProceso;
            ViewBag.PagosDesaprobados = pagosDesaprobados;
            ViewBag.TotalRecaudado = totalRecaudado;

            ViewBag.TopProductos = topProductos;
            ViewBag.ProductosStockCritico = productosStockCritico;

            return View();
        }

        private int? ObtenerIdTiendaVendedorLogueado()
        {
            var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuario == null)
            {
                return null;
            }

            // Primero intenta por la nueva estructura: usuario -> miembro_tienda -> tienda
            var idTiendaPorMiembro = _context.MiembroTiendas
                .Where(m => m.IdUsuario == idUsuario.Value)
                .Select(m => (int?)m.IdTienda)
                .FirstOrDefault();

            if (idTiendaPorMiembro != null)
            {
                return idTiendaPorMiembro;
            }

            // Fallback al flujo actual: usuario -> vendedor -> tienda
            var idTienda = _context.Tiendas
                .Include(t => t.IdVendedorNavigation)
                .Where(t => t.IdVendedorNavigation.IdUsuario == idUsuario.Value)
                .Select(t => (int?)t.IdTienda)
                .FirstOrDefault();

            return idTienda;
        }
    }
}