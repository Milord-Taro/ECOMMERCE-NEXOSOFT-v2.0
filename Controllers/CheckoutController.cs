using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Helpers;
using ECOMMERCE_NEXOSOFT.Models;
using ECOMMERCE_NEXOSOFT.Filters;
using Microsoft.AspNetCore.Mvc;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(2, 3)]
    public class CheckoutController : Controller
    {
        private readonly NexosoftDbContext _context;
        private const string CarritoKey = "CARRITO";

        public CheckoutController(NexosoftDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItem>>(CarritoKey) ?? new List<CarritoItem>();

            if (!carrito.Any())
            {
                return RedirectToAction("Index", "Carrito");
            }

            return View(carrito);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmar()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItem>>(CarritoKey) ?? new List<CarritoItem>();

            if (!carrito.Any())
            {
                return RedirectToAction("Index", "Carrito");
            }

            var idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuarioSesion == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            foreach (var item in carrito)
            {
                var stock = _context.Stocks.FirstOrDefault(s => s.IdProducto == item.IdProducto);

                if (stock == null || item.Cantidad > stock.StockActual)
                {
                    TempData["MensajeCheckout"] = $"Stock insuficiente para el producto: {item.NombreProducto}";
                    return RedirectToAction("Index");
                }

                if (item.IdTienda == null)
                {
                    TempData["MensajeCheckout"] = $"El producto {item.NombreProducto} no tiene una tienda válida asociada.";
                    return RedirectToAction("Index");
                }
            }

            var random = new Random();
            var gruposPorTienda = carrito.GroupBy(x => x.IdTienda).ToList();
            var pedidosCreados = new List<int>();

            foreach (var grupo in gruposPorTienda)
            {
                var subtotal = grupo.Sum(x => x.Precio * x.Cantidad);
                var costoEnvio = 0m;
                var total = subtotal + costoEnvio;

                var pedido = new Pedido
                {
                    CodPedido = random.Next(100000, 999999),
                    IdUsuario = idUsuarioSesion.Value,
                    IdTienda = grupo.Key,
                    FechaCreacion = DateTime.Now,
                    Subtotal = subtotal,
                    CostoEnvio = costoEnvio,
                    Total = total,
                    MetodoEntrega = "domicilio",
                    EstadoPedido = "pendiente"
                };

                _context.Pedidos.Add(pedido);
                _context.SaveChanges();

                foreach (var item in grupo)
                {
                    var detalle = new Detallepedido
                    {
                        CodDetallePedido = random.Next(100000, 999999),
                        IdPedido = pedido.IdPedido,
                        IdProducto = item.IdProducto,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio,
                        Subtotal = item.Precio * item.Cantidad
                    };

                    _context.Detallepedidos.Add(detalle);

                    var stock = _context.Stocks.FirstOrDefault(s => s.IdProducto == item.IdProducto);
                    if (stock != null)
                    {
                        stock.StockActual -= item.Cantidad;
                    }
                }

                _context.SaveChanges();
                pedidosCreados.Add(pedido.IdPedido);
            }

            HttpContext.Session.Remove(CarritoKey);

            return RedirectToAction("ConfirmacionMultiple", new { ids = string.Join(",", pedidosCreados) });
        }

        public IActionResult Confirmacion(int id)
        {
            var pedido = _context.Pedidos.FirstOrDefault(p => p.IdPedido == id);

            if (pedido == null)
            {
                return NotFound();
            }

            return View(pedido);
        }

        public IActionResult ConfirmacionMultiple(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return RedirectToAction("Index", "Productos");
            }

            var listaIds = ids
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var valor) ? valor : 0)
                .Where(id => id > 0)
                .ToList();

            var pedidos = _context.Pedidos
                .Where(p => listaIds.Contains(p.IdPedido))
                .OrderByDescending(p => p.FechaCreacion)
                .ToList();

            return View(pedidos);
        }
    }
}