using ECOMMERCE_NEXOSOFT.Models;

namespace ECOMMERCE_NEXOSOFT.Helpers
{
    public static class CarritoHelper
    {
        public static int ObtenerCantidadItems(List<CarritoItem>? carrito)
        {
            if (carrito == null || !carrito.Any())
            {
                return 0;
            }

            return carrito.Sum(x => x.Cantidad);
        }
    }
}