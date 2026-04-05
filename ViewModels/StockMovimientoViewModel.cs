using System.ComponentModel.DataAnnotations;

namespace ECOMMERCE_NEXOSOFT.ViewModels
{
    public class StockMovimientoViewModel
    {
        public int IdInventario { get; set; }

        public int CodInventario { get; set; }

        public int IdProducto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public decimal PrecioCompraStock { get; set; }

        public int StockActual { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public int StockMinimo { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un tipo de movimiento.")]
        public string TipoMovimiento { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa.")]
        public int Cantidad { get; set; }

        [StringLength(150, ErrorMessage = "El motivo no puede superar 150 caracteres.")]
        public string? Motivo { get; set; }
    }
}