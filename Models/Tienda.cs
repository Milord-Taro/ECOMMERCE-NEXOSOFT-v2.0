using System.ComponentModel.DataAnnotations;
using ECOMMERCE_NEXOSOFT.Helpers;

namespace ECOMMERCE_NEXOSOFT.Models
{
    public partial class Tienda
    {
        public int IdTienda { get; set; }

        public int CodTienda { get; set; }

        public int IdVendedor { get; set; }

        [Required(ErrorMessage = "El nombre de la tienda es obligatorio.")]
        [RegularExpression(ValidationRules.StoreNamePattern, ErrorMessage = "El nombre de la tienda debe tener entre 2 y 30 caracteres y solo puede contener letras, números, espacios, &, - y .")]
        public string NombreTienda { get; set; } = null!;

        [StringLength(300, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 300 caracteres.")]
        public string? Descripcion { get; set; }

        [StringLength(500, ErrorMessage = "La URL del logo no puede superar 500 caracteres.")]
        public string? LogoUrl { get; set; }

        public bool VisiblePublico { get; set; }

        public DateTime FechaRegistro { get; set; }
        [StringLength(150)]
        public string? RazonSocial { get; set; }

        [StringLength(10)]
        public string? NitRut { get; set; }

        [StringLength(100)]
        public string? NombreRepresentante { get; set; }

        [StringLength(10)]
        public string? TelefonoContacto { get; set; }

        [StringLength(100)]
        public string? CorreoContacto { get; set; }

        [StringLength(150)]
        public string? DireccionComercial { get; set; }

        public virtual Vendedor IdVendedorNavigation { get; set; } = null!;

        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

        public virtual ICollection<MiembroTienda> MiembroTiendas { get; set; } = new List<MiembroTienda>();
    }
}