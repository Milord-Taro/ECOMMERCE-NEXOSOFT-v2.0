using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int CodUsuario { get; set; }

    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string TipoIdentificacion { get; set; } = null!;

    public string NumeroIdentificacion { get; set; } = null!;

    public string? Telefono { get; set; }

    public string CorreoElectronico { get; set; } = null!;

    public string Contrasena { get; set; } = null!;


    public DateOnly? FechaRegistro { get; set; }

    public virtual Cliente? Cliente { get; set; }

    public virtual Vendedor? Vendedor { get; set; }

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public virtual ICollection<SolicitudVendedor> SolicitudVendedors { get; set; } = new List<SolicitudVendedor>();
}
