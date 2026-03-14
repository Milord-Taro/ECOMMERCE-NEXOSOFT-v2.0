using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Detallepedido
{
    public int IdDetallePedido { get; set; }

    public int CodDetallePedido { get; set; }

    public int IdPedido { get; set; }

    public int IdProducto { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Pedido IdPedidoNavigation { get; set; } = null!;

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
