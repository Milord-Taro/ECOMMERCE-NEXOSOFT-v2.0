using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Ventum
{
    public int IdVenta { get; set; }

    public int CodVenta { get; set; }

    public int IdCliente { get; set; }

    public int IdPedido { get; set; }

    public DateTime FechaVenta { get; set; }

    public string EstadoVenta { get; set; } = null!;

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Pedido IdPedidoNavigation { get; set; } = null!;

    public virtual Pago? Pago { get; set; }
}
