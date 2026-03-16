using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Helpers;
using ECOMMERCE_NEXOSOFT.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
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

            foreach (var item in carrito)
            {
                var stock = _context.Stocks.FirstOrDefault(s => s.IdProducto == item.IdProducto);

                if (stock == null || item.Cantidad > stock.StockActual)
                {
                    TempData["MensajeCheckout"] = $"Stock insuficiente para el producto: {item.NombreProducto}";
                    return RedirectToAction("Index");
                }
            }

            var subtotal = carrito.Sum(x => x.Precio * x.Cantidad);
            var costoEnvio = 0m;
            var total = subtotal + costoEnvio;

            var random = new Random();

            var pedido = new Pedido
            {
                CodPedido = random.Next(100000, 999999),
                IdUsuario = 1, // temporal hasta implementar login
                FechaCreacion = DateTime.Now,
                Subtotal = subtotal,
                CostoEnvio = costoEnvio,
                Total = total,
                MetodoEntrega = "domicilio",
                EstadoPedido = "pendiente"
            };

            _context.Pedidos.Add(pedido);
            _context.SaveChanges();

            foreach (var item in carrito)
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

            HttpContext.Session.Remove(CarritoKey);

            return RedirectToAction("Confirmacion", new { id = pedido.IdPedido });
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
    }
}