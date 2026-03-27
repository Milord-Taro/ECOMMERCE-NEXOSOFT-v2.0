using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(1)]
    public class AdminController : Controller
    {
        private readonly NexosoftDbContext _context;

        public AdminController(NexosoftDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsuarios = await _context.Usuarios.CountAsync();
            ViewBag.TotalClientes = await _context.Usuarios.CountAsync(u => u.IdRol == 2);
            ViewBag.TotalVendedores = await _context.Usuarios.CountAsync(u => u.IdRol == 3);

            ViewBag.TotalTiendas = await _context.Tiendas.CountAsync();
            ViewBag.TiendasVisibles = await _context.Tiendas.CountAsync(t => t.VisiblePublico);
            ViewBag.TiendasOcultas = await _context.Tiendas.CountAsync(t => !t.VisiblePublico);

            ViewBag.TotalProductos = await _context.Productos.CountAsync();
            ViewBag.ProductosPublicados = await _context.Productos.CountAsync(p => p.VisiblePublico);
            ViewBag.ProductosOcultos = await _context.Productos.CountAsync(p => !p.VisiblePublico);

            ViewBag.SolicitudesPendientes = await _context.SolicitudVendedors
                .CountAsync(s => s.EstadoSolicitud == "pendiente");

            ViewBag.UltimosPedidos = await _context.Pedidos
                .Include(p => p.IdTiendaNavigation)
                .Include(p => p.IdUsuarioNavigation)
                .OrderByDescending(p => p.FechaCreacion)
                .Take(5)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> Metricas()
        {
            var totalUsuarios = await _context.Usuarios.CountAsync();
            var totalClientes = await _context.Usuarios.CountAsync(u => u.IdRol == 2);
            var totalVendedores = await _context.Usuarios.CountAsync(u => u.IdRol == 3);

            var totalTiendas = await _context.Tiendas.CountAsync();
            var tiendasVisibles = await _context.Tiendas.CountAsync(t => t.VisiblePublico);
            var tiendasOcultas = await _context.Tiendas.CountAsync(t => !t.VisiblePublico);

            var totalProductos = await _context.Productos.CountAsync();
            var productosPublicados = await _context.Productos.CountAsync(p => p.VisiblePublico);
            var productosOcultos = await _context.Productos.CountAsync(p => !p.VisiblePublico);

            var solicitudesPendientes = await _context.SolicitudVendedors.CountAsync(s => s.EstadoSolicitud == "pendiente");
            var solicitudesAprobadas = await _context.SolicitudVendedors.CountAsync(s => s.EstadoSolicitud == "aprobada");
            var solicitudesRechazadas = await _context.SolicitudVendedors.CountAsync(s => s.EstadoSolicitud == "rechazada");

            var totalPedidos = await _context.Pedidos.CountAsync();
            var pedidosUltimos7Dias = await _context.Pedidos.CountAsync(p => p.FechaCreacion >= DateTime.Now.AddDays(-7));
            var pedidosUltimos30Dias = await _context.Pedidos.CountAsync(p => p.FechaCreacion >= DateTime.Now.AddDays(-30));

            var totalVentas = await _context.Venta.CountAsync();
            var pagosAprobados = await _context.Pagos.CountAsync(p => p.EstadoPago == "aprobado");
            var pagosEnProceso = await _context.Pagos.CountAsync(p => p.EstadoPago == "enproceso");
            var pagosDesaprobados = await _context.Pagos.CountAsync(p => p.EstadoPago == "desaprobado");

            var recaudoTotal = await _context.Pagos
                .Where(p => p.EstadoPago == "aprobado")
                .SumAsync(p => (decimal?)p.MontoPagado) ?? 0m;

            var topTiendasPorPedidos = await _context.Pedidos
                .Include(p => p.IdTiendaNavigation)
                .Where(p => p.IdTiendaNavigation != null)
                .GroupBy(p => p.IdTiendaNavigation!.NombreTienda)
                .Select(g => new
                {
                    NombreTienda = g.Key,
                    TotalPedidos = g.Count(),
                    TotalMonto = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.TotalPedidos)
                .Take(5)
                .ToListAsync();

            var topTiendasPorRecaudo = await _context.Pagos
                .Include(p => p.IdVentaNavigation)
                    .ThenInclude(v => v.IdPedidoNavigation)
                        .ThenInclude(pe => pe.IdTiendaNavigation)
                .Where(p => p.EstadoPago == "aprobado" &&
                            p.IdVentaNavigation != null &&
                            p.IdVentaNavigation.IdPedidoNavigation != null &&
                            p.IdVentaNavigation.IdPedidoNavigation.IdTiendaNavigation != null)
                .GroupBy(p => p.IdVentaNavigation.IdPedidoNavigation.IdTiendaNavigation!.NombreTienda)
                .Select(g => new
                {
                    NombreTienda = g.Key,
                    TotalRecaudado = g.Sum(x => x.MontoPagado)
                })
                .OrderByDescending(x => x.TotalRecaudado)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.TotalClientes = totalClientes;
            ViewBag.TotalVendedores = totalVendedores;

            ViewBag.TotalTiendas = totalTiendas;
            ViewBag.TiendasVisibles = tiendasVisibles;
            ViewBag.TiendasOcultas = tiendasOcultas;

            ViewBag.TotalProductos = totalProductos;
            ViewBag.ProductosPublicados = productosPublicados;
            ViewBag.ProductosOcultos = productosOcultos;

            ViewBag.SolicitudesPendientes = solicitudesPendientes;
            ViewBag.SolicitudesAprobadas = solicitudesAprobadas;
            ViewBag.SolicitudesRechazadas = solicitudesRechazadas;

            ViewBag.TotalPedidos = totalPedidos;
            ViewBag.PedidosUltimos7Dias = pedidosUltimos7Dias;
            ViewBag.PedidosUltimos30Dias = pedidosUltimos30Dias;

            ViewBag.TotalVentas = totalVentas;
            ViewBag.PagosAprobados = pagosAprobados;
            ViewBag.PagosEnProceso = pagosEnProceso;
            ViewBag.PagosDesaprobados = pagosDesaprobados;
            ViewBag.RecaudoTotal = recaudoTotal;

            ViewBag.TopTiendasPorPedidos = topTiendasPorPedidos;
            ViewBag.TopTiendasPorRecaudo = topTiendasPorRecaudo;

            return View();
        }
    }
}