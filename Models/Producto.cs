using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Producto
{
    public int IdProducto { get; set; }

    public int CodProducto { get; set; }

    public int IdCategoria { get; set; }

    public int? IdSubcategoria { get; set; }

    public int? IdTienda { get; set; }

    public string NombreProducto { get; set; } = null!;

    public string? DescripcionCorta { get; set; }

    public string SkuProducto { get; set; } = null!;

    public string CodigoBarrasProducto { get; set; } = null!;

    public string UnidadMedidaProducto { get; set; } = null!;

    public string? MarcaProducto { get; set; }

    public bool Favorito { get; set; }

    public bool VisiblePublico { get; set; }

    public decimal PrecioVentaProducto { get; set; }

    public virtual ICollection<Detallepedido> Detallepedidos { get; set; } = new List<Detallepedido>();

    public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

    public virtual Subcategorium? IdSubcategoriaNavigation { get; set; }

    public virtual Tienda? IdTiendaNavigation { get; set; }

    public virtual Stock? Stock { get; set; }
}
