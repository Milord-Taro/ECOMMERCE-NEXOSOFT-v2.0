using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models;

public partial class Pedido
{
    public int IdPedido { get; set; }

    public int CodPedido { get; set; }

    public int IdUsuario { get; set; }

    public int? IdTienda { get; set; }

    public DateTime FechaCreacion { get; set; }

    public decimal Subtotal { get; set; }

    public decimal CostoEnvio { get; set; }

    public decimal Total { get; set; }

    public string? MetodoEntrega { get; set; }

    public string? DireccionEntrega { get; set; }

    public string? TelefonoEntrega { get; set; }

    public string EstadoPedido { get; set; } = null!;

    public virtual ICollection<Detallepedido> Detallepedidos { get; set; } = new List<Detallepedido>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual Tienda? IdTiendaNavigation { get; set; }

    public virtual Ventum? Ventum { get; set; }
}
