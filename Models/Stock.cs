using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Stock
{
    public int IdInventario { get; set; }

    public int CodInventario { get; set; }

    public int IdProducto { get; set; }

    public decimal PrecioCompraStock { get; set; }

    public int StockActual { get; set; }

    public int StockMinimo { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
