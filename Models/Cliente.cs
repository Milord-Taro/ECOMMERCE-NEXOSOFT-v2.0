using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public int CodCliente { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaRegistroCliente { get; set; }

    public string EstadoCliente { get; set; } = null!;

    public string? Direccion1 { get; set; }

    public string? Direccion2 { get; set; }

    public string? Direccion3 { get; set; }

    public virtual ICollection<Domicilio> Domicilios { get; set; } = new List<Domicilio>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Ventum> Venta { get; set; } = new List<Ventum>();
}
