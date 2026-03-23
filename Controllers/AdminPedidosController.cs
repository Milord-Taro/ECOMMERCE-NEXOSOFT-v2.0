using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
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
    }
}