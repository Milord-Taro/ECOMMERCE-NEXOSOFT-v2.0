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
        private const string CheckoutDireccionKey = "CHECKOUT_DIRECCION";
        private const string CheckoutTelefonoKey = "CHECKOUT_TELEFONO";
        private const string CheckoutMetodoPagoKey = "CHECKOUT_METODO_PAGO";

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

        private void LimpiarCheckoutPendiente()
        {
            HttpContext.Session.Remove(CheckoutDireccionKey);
            HttpContext.Session.Remove(CheckoutTelefonoKey);
            HttpContext.Session.Remove(CheckoutMetodoPagoKey);
        }

        private IActionResult ProcesarCompra(string direccionEntrega, string telefonoEntrega, string metodoPago)
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

            if (string.IsNullOrWhiteSpace(direccionEntrega))
            {
                TempData["MensajeCheckout"] = "Debes seleccionar una dirección de entrega.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(telefonoEntrega))
            {
                TempData["MensajeCheckout"] = "No se encontró un teléfono de contacto válido.";
                return RedirectToAction("Index");
            }

            var metodosPagoValidos = new[] { "efectivo", "tarjeta credito", "tarjeta debito", "transferencias" };

            if (string.IsNullOrWhiteSpace(metodoPago) || !metodosPagoValidos.Contains(metodoPago))
            {
                TempData["MensajeCheckout"] = "Debes seleccionar un método de pago válido.";
                return RedirectToAction("Index");
            }

            var clienteComprador = ObtenerOCrearClienteComprador(idUsuarioSesion.Value);

            var direccionesValidas = new List<string>();

            if (!string.IsNullOrWhiteSpace(clienteComprador.Direccion1))
                direccionesValidas.Add(clienteComprador.Direccion1.Trim());

            if (!string.IsNullOrWhiteSpace(clienteComprador.Direccion2))
                direccionesValidas.Add(clienteComprador.Direccion2.Trim());

            if (!string.IsNullOrWhiteSpace(clienteComprador.Direccion3))
                direccionesValidas.Add(clienteComprador.Direccion3.Trim());

            if (!direccionesValidas.Contains(direccionEntrega.Trim()))
            {
                TempData["MensajeCheckout"] = "La dirección seleccionada no es válida.";
                return RedirectToAction("Index");
            }

            var usuarioComprador = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idUsuarioSesion.Value);

            if (usuarioComprador == null || string.IsNullOrWhiteSpace(usuarioComprador.Telefono))
            {
                TempData["MensajeCheckout"] = "No se encontró un teléfono de contacto válido.";
                return RedirectToAction("Index");
            }

            if (telefonoEntrega.Trim() != usuarioComprador.Telefono.Trim())
            {
                TempData["MensajeCheckout"] = "El teléfono enviado no coincide con el registrado en tu cuenta.";
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
                    DireccionEntrega = direccionEntrega.Trim(),
                    TelefonoEntrega = telefonoEntrega.Trim(),
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

                var descripcionPago = metodoPago == "efectivo"
                    ? "Pago contra entrega registrado."
                    : "Pago demo aprobado por la pasarela simulada.";

                var pago = new Pago
                {
                    CodPago = nuevoCodPago,
                    IdVenta = venta.IdVenta,
                    MetodoPago = metodoPago,
                    Descripcion = descripcionPago,
                    FechaPago = DateTime.Now,
                    MontoPagado = total,
                    CodigoAutorizacion = codigoAutorizacion,
                    EstadoPago = "aprobado"
                };

                _context.Pagos.Add(pago);

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

            LimpiarCheckoutPendiente();
            HttpContext.Session.Remove(CarritoKey);

            return RedirectToAction("ConfirmacionMultiple", new { ids = string.Join(",", pedidosCreados) });
        }

        public IActionResult Index()
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

            var usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == idUsuarioSesion.Value);
            var cliente = _context.Clientes.FirstOrDefault(c => c.IdUsuario == idUsuarioSesion.Value);

            var direcciones = new List<string>();

            if (cliente != null)
            {
                if (!string.IsNullOrWhiteSpace(cliente.Direccion1))
                    direcciones.Add(cliente.Direccion1.Trim());

                if (!string.IsNullOrWhiteSpace(cliente.Direccion2))
                    direcciones.Add(cliente.Direccion2.Trim());

                if (!string.IsNullOrWhiteSpace(cliente.Direccion3))
                    direcciones.Add(cliente.Direccion3.Trim());
            }

            ViewBag.TelefonoEntrega = usuario?.Telefono ?? string.Empty;
            ViewBag.DireccionesEntrega = direcciones;

            if (!direcciones.Any())
            {
                TempData["MensajeCheckout"] = "No tienes direcciones registradas. Actualiza tu perfil de cliente antes de continuar con la compra.";
            }

            if (string.IsNullOrWhiteSpace(usuario?.Telefono))
            {
                TempData["MensajeCheckout"] = "No tienes un teléfono registrado. Actualiza tu perfil antes de continuar con la compra.";
            }

            return View(carrito);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmar(string direccionEntrega, string telefonoEntrega, string metodoPago)
        {
            if (string.IsNullOrWhiteSpace(direccionEntrega) ||
                string.IsNullOrWhiteSpace(telefonoEntrega) ||
                string.IsNullOrWhiteSpace(metodoPago))
            {
                TempData["MensajeCheckout"] = "Debes completar los datos de entrega y seleccionar un método de pago.";
                return RedirectToAction("Index");
            }

            if (metodoPago == "efectivo")
            {
                return ProcesarCompra(direccionEntrega, telefonoEntrega, metodoPago);
            }

            HttpContext.Session.SetString(CheckoutDireccionKey, direccionEntrega.Trim());
            HttpContext.Session.SetString(CheckoutTelefonoKey, telefonoEntrega.Trim());
            HttpContext.Session.SetString(CheckoutMetodoPagoKey, metodoPago.Trim());

            return RedirectToAction(nameof(PasarelaDemo));
        }

        public IActionResult PasarelaDemo()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItem>>(CarritoKey) ?? new List<CarritoItem>();

            if (!carrito.Any())
            {
                return RedirectToAction("Index", "Carrito");
            }

            var metodoPago = HttpContext.Session.GetString(CheckoutMetodoPagoKey);

            if (string.IsNullOrWhiteSpace(metodoPago))
            {
                TempData["MensajeCheckout"] = "No hay un pago pendiente para procesar.";
                return RedirectToAction("Index");
            }

            ViewBag.MetodoPago = metodoPago;
            ViewBag.Total = carrito.Sum(x => x.Precio * x.Cantidad);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AutorizarPagoDemo(string? titularPago, string? referenciaPago)
        {
            var direccionEntrega = HttpContext.Session.GetString(CheckoutDireccionKey);
            var telefonoEntrega = HttpContext.Session.GetString(CheckoutTelefonoKey);
            var metodoPago = HttpContext.Session.GetString(CheckoutMetodoPagoKey);

            if (string.IsNullOrWhiteSpace(direccionEntrega) ||
                string.IsNullOrWhiteSpace(telefonoEntrega) ||
                string.IsNullOrWhiteSpace(metodoPago))
            {
                TempData["MensajeCheckout"] = "No hay información de pago pendiente para procesar.";
                return RedirectToAction("Index");
            }

            if ((metodoPago == "tarjeta debito" || metodoPago == "tarjeta credito") &&
                string.IsNullOrWhiteSpace(titularPago))
            {
                TempData["MensajeCheckout"] = "Debes ingresar el nombre del titular para continuar con el pago demo.";
                return RedirectToAction(nameof(PasarelaDemo));
            }

            if (metodoPago == "transferencias" && string.IsNullOrWhiteSpace(referenciaPago))
            {
                TempData["MensajeCheckout"] = "Debes ingresar una referencia para continuar con la transferencia demo.";
                return RedirectToAction(nameof(PasarelaDemo));
            }

            return ProcesarCompra(direccionEntrega, telefonoEntrega, metodoPago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelarPagoDemo()
        {
            LimpiarCheckoutPendiente();
            TempData["MensajeCheckout"] = "El pago demo fue cancelado.";
            return RedirectToAction("Index");
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