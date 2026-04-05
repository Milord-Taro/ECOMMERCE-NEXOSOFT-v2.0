using System.ComponentModel.DataAnnotations;
using ECOMMERCE_NEXOSOFT.Helpers;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public int CodProducto { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public int IdCategoria { get; set; }

    public int? IdSubcategoria { get; set; }

    public int? IdTienda { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del producto debe tener entre 3 y 50 caracteres.")]
    public string NombreProducto { get; set; } = null!;

    [Required(ErrorMessage = "La descripción corta es obligatoria.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "La descripción corta debe tener entre 3 y 150 caracteres.")]
    public string? DescripcionCorta { get; set; }

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [RegularExpression(ValidationRules.SkuPattern, ErrorMessage = "El SKU solo puede contener letras, números y guion, sin espacios.")]
    public string SkuProducto { get; set; } = null!;

    [Required(ErrorMessage = "El código de barras es obligatorio.")]
    [RegularExpression(ValidationRules.BarcodePattern, ErrorMessage = "El código de barras debe contener solo números y entre 8 y 13 dígitos.")]
    public string CodigoBarrasProducto { get; set; } = null!;

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    public string UnidadMedidaProducto { get; set; } = null!;

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "La marca debe tener entre 2 y 50 caracteres.")]
    public string? MarcaProducto { get; set; }

    public bool Favorito { get; set; }

    public bool VisiblePublico { get; set; }

    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "El precio de venta debe ser mayor a 0.")]
    public decimal PrecioVentaProducto { get; set; }

    public virtual ICollection<Detallepedido> Detallepedidos { get; set; } = new List<Detallepedido>();

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

    public virtual Subcategorium? IdSubcategoriaNavigation { get; set; }

    public virtual Tienda? IdTiendaNavigation { get; set; }

    public virtual Stock? Stock { get; set; }
}