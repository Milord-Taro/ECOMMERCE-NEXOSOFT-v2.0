using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Facturacion
{
    public int IdFactura { get; set; }

    public int CodFactura { get; set; }

    public int IdPago { get; set; }

    public string NumeroFactura { get; set; } = null!;

    public DateTime FechaFactura { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Impuestos { get; set; }

    public decimal TotalFactura { get; set; }

    public string EstadoFactura { get; set; } = null!;

    public virtual Pago IdPagoNavigation { get; set; } = null!;
}
