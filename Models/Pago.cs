using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Pago
{
    public int IdPago { get; set; }

    public int CodPago { get; set; }

    public int IdVenta { get; set; }

    public string MetodoPago { get; set; } = null!;

    public string? Descripcion { get; set; }

    public DateTime FechaPago { get; set; }

    public decimal MontoPagado { get; set; }

    public string CodigoAutorizacion { get; set; } = null!;

    public string EstadoPago { get; set; } = null!;

    public virtual Facturacion? Facturacion { get; set; }

    public virtual Ventum IdVentaNavigation { get; set; } = null!;
}
