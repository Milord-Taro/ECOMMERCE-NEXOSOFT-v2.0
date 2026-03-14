using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Domicilio
{
    public int IdDomicilio { get; set; }

    public int CodDomicilio { get; set; }

    public int IdCliente { get; set; }

    public string DireccionEntrega { get; set; } = null!;

    public string? Transportadora { get; set; }

    public DateOnly FechaEnvio { get; set; }

    public DateTime HoraEnvio { get; set; }

    public decimal CostoEnvio { get; set; }

    public string EstadoEnvio { get; set; } = null!;

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
