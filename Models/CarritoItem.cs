namespace ECOMMERCE_NEXOSOFT.Models
{
    public class CarritoItem
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string? ImagenUrl { get; set; }
        public int? IdTienda { get; set; }
        public string? NombreTienda { get; set; }
    }
}