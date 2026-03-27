using System.ComponentModel.DataAnnotations;

namespace ECOMMERCE_NEXOSOFT.ViewModels
{
    public class SolicitudVendedorViewModel
    {
        [Required(ErrorMessage = "El nombre de la tienda es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre de la tienda no puede superar 100 caracteres")]
        public string NombreTiendaSolicitada { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "La descripción no puede superar 150 caracteres")]
        public string? DescripcionTienda { get; set; }
    }
}