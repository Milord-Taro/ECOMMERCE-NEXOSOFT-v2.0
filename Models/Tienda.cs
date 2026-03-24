using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models
{
    public partial class Tienda
    {
        public int IdTienda { get; set; }
        public int CodTienda { get; set; }
        public int IdVendedor { get; set; }
        public string NombreTienda { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? LogoUrl { get; set; }
        public bool VisiblePublico { get; set; }
        public DateTime FechaRegistro { get; set; }

        public virtual Vendedor IdVendedorNavigation { get; set; } = null!;
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
        public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}