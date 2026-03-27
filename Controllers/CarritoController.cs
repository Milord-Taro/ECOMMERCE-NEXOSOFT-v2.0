using ECOMMERCE_NEXOSOFT.Data;
using ECOMMERCE_NEXOSOFT.Filters;
using ECOMMERCE_NEXOSOFT.Helpers;
using ECOMMERCE_NEXOSOFT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECOMMERCE_NEXOSOFT.Controllers
{
    [AuthorizeUser(2, 3)]
    public class CarritoController : Controller
    {
        private readonly NexosoftDbContext _context;
        private const string CarritoKey = "CARRITO";

        public CarritoController(NexosoftDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItem>>(CarritoKey) ?? new List<CarritoItem>();
            return View(carrito);
        }

        public IActionResult Agregar(int id, int cantidad = 1)
        {
            var producto = _context.Productos
                .Include(p => p.IdTiendaNavigation)
                .FirstOrDefault(p => p.IdProducto == id);

            if (producto == null)
            {
                return NotFound();
            }

            if (cantidad < 1)
            {
                cantidad = 1;
            }

            var stockProducto = _context.Stocks
                .FirstOrDefault(s => s.IdProducto == id);

            if (stockProducto == null || stockProducto.StockActual <= 0)
            {
                TempData["MensajeCarrito"] = "Este producto no tiene stock disponible.";
                return RedirectToAction("Detalle", "Productos", new { id = id });
            }

            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItem>>(CarritoKey) ?? new List<CarritoItem>();

            var itemExistente = carrito.FirstOrDefault(p => p.IdProducto == id);

            if (itemExistente != null)
            {
                var nuevaCantidad = itemExistente.Cantidad + cantidad;

                if (nuevaCantidad <= stockProducto.StockActual)
                {
                    itemExistente.Cantidad = nuevaCantidad;
                }
                else
                {
                    itemExistente.Cantidad = stockProducto.StockActual;
                    TempData["MensajeCarrito"] = "Has alcanzado el stock máximo disponible de este producto.";
                }
            }
            else
            {
                var cantidadFinal = cantidad;

                if (cantidadFinal > stockProducto.StockActual)
                {
                    cantidadFinal = stockProducto.StockActual;
                    TempData["MensajeCarrito"] = "La cantidad solicitada supera el stock disponible. Se agregó la cantidad máxima permitida.";
                }

                carrito.Add(new CarritoItem
                {
                    IdProducto = producto.IdProducto,
                    NombreProducto = producto.NombreProducto,
                    Precio = producto.PrecioVentaProducto,
                    Cantidad = cantidadFinal,
                    ImagenUrl = "/img/producto-default.jpg",
                    IdTienda = producto.IdTienda,
                    NombreTienda = producto.IdTiendaNavigation?.NombreTienda
                });
            }

            HttpContext.Session.SetObjectAsJson(CarritoKey, carrito);

            return RedirectToAction("Index");
        }

        public IActionResult Eliminar(int id)
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItem>>(CarritoKey) ?? new List<CarritoItem>();

            var item = carrito.FirstOrDefault(p => p.IdProducto == id);

            if (item != null)
            {
                carrito.Remove(item);
            }

            HttpContext.Session.SetObjectAsJson(CarritoKey, carrito);

            return RedirectToAction("Index");
        }

        public IActionResult Aumentar(int id)
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItem>>(CarritoKey) ?? new List<CarritoItem>();

            var item = carrito.FirstOrDefault(p => p.IdProducto == id);

            var stockProducto = _context.Stocks
                .FirstOrDefault(s => s.IdProducto == id);

            if (item != null && stockProducto != null)
            {
                if (item.Cantidad < stockProducto.StockActual)
                {
                    item.Cantidad++;
                }
                else
                {
                    TempData["MensajeCarrito"] = "Has alcanzado el stock máximo disponible de este producto.";
                }
            }

            HttpContext.Session.SetObjectAsJson(CarritoKey, carrito);

            return RedirectToAction("Index");
        }

        public IActionResult Disminuir(int id)
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItem>>(CarritoKey) ?? new List<CarritoItem>();

            var item = carrito.FirstOrDefault(p => p.IdProducto == id);

            if (item != null)
            {
                item.Cantidad--;

                if (item.Cantidad <= 0)
                {
                    carrito.Remove(item);
                }
            }

            HttpContext.Session.SetObjectAsJson(CarritoKey, carrito);

            return RedirectToAction("Index");
        }

        public IActionResult Vaciar()
        {
            HttpContext.Session.Remove(CarritoKey);
            return RedirectToAction("Index");
        }
    }
}