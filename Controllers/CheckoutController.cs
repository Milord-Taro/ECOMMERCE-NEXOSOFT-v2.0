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

        private Cliente ObtenerOCrearClienteComprador(int idUsuario)
        {
            var cliente = _context.Clientes.FirstOrDefault(c => c.IdUsuario == idUsuario);

            if (cliente != null)
            {
                return cliente;
            }

            var ultimoCodCliente = _context.Clientes
                .OrderByDescending(c => c.CodCliente)
                .Select(c => (int?)c.CodCliente)
                .FirstOrDefault();

            int nuevoCodCliente = (ultimoCodCliente ?? 2000) + 1;

            cliente = new Cliente
            {
                CodCliente = nuevoCodCliente,
                IdUsuario = idUsuario,
                FechaRegistroCliente = DateTime.Now,
                EstadoCliente = "activo"
            };

            _context.Clientes.Add(cliente);
            _context.SaveChanges();

            return cliente;
        }

        public IActionResult Index()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItem>>(CarritoKey) ?? new List<CarritoItem>();

            if (!carrito.Any())
            {
                return RedirectToAction("Index", "Carrito");
            }

            var idUsuarioSesion = HttpContext.Session.GetInt32("IdUsuario");

            if (idUsuarioSesion != null)
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idUsuarioSesion.Value);
                var cliente = _context.Clientes.FirstOrDefault(c => c.IdUsuario == idUsuarioSesion.Value);

                ViewBag.TelefonoEntrega = usuario?.Telefono ?? string.Empty;
                ViewBag.DireccionEntrega = cliente?.Direccion1 ?? string.Empty;
            }
            else
            {
                ViewBag.TelefonoEntrega = string.Empty;
                ViewBag.DireccionEntrega = string.Empty;
            }

            return View(carrito);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmar(string direccionEntrega, string telefonoEntrega, string metodoPago)
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

            var clienteComprador = ObtenerOCrearClienteComprador(idUsuarioSesion.Value);

            if (string.IsNullOrWhiteSpace(direccionEntrega))
            {
                TempData["MensajeCheckout"] = "Debes ingresar una dirección de entrega.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(telefonoEntrega))
            {
                TempData["MensajeCheckout"] = "Debes ingresar un teléfono de contacto.";
                return RedirectToAction("Index");
            }

            var metodosPagoValidos = new[] { "efectivo", "tarjeta credito", "tarjeta debito", "transferencias" };

            if (string.IsNullOrWhiteSpace(metodoPago) || !metodosPagoValidos.Contains(metodoPago))
            {
                TempData["MensajeCheckout"] = "Debes seleccionar un método de pago válido.";
                return RedirectToAction("Index");
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
                    DireccionEntrega = direccionEntrega,
                    TelefonoEntrega = telefonoEntrega,
                    EstadoPedido = "pendiente"
                };

                _context.Pedidos.Add(pedido);
                _context.SaveChanges();

                var ultimoCodVenta = _context.Venta
                    .OrderByDescending(v => v.CodVenta)
                    .Select(v => (int?)v.CodVenta)
                    .FirstOrDefault();

                int nuevoCodVenta = (ultimoCodVenta ?? 8000) + 1;

                var venta = new Ventum
                {
                    CodVenta = nuevoCodVenta,
                    IdCliente = clienteComprador.IdCliente,
                    IdPedido = pedido.IdPedido,
                    FechaVenta = DateTime.Now,
                    EstadoVenta = "pagada"
                };

                _context.Venta.Add(venta);
                _context.SaveChanges();

                var ultimoCodPago = _context.Pagos
                    .OrderByDescending(p => p.CodPago)
                    .Select(p => (int?)p.CodPago)
                    .FirstOrDefault();

                int nuevoCodPago = (ultimoCodPago ?? 9000) + 1;

                var codigoAutorizacion = "AUT" + Guid.NewGuid().ToString("N")[..8].ToUpper();

                var pago = new Pago
                {
                    CodPago = nuevoCodPago,
                    IdVenta = venta.IdVenta,
                    MetodoPago = metodoPago,
                    Descripcion = "Pago demo aprobado por la pasarela simulada.",
                    FechaPago = DateTime.Now,
                    MontoPagado = total,
                    CodigoAutorizacion = codigoAutorizacion,
                    EstadoPago = "aprobado"
                };

                _context.Pagos.Add(pago);
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