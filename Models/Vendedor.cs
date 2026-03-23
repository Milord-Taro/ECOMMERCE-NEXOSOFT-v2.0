using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models
{
    public partial class Vendedor
    {
        public int IdVendedor { get; set; }
        public int CodVendedor { get; set; }
        public int IdUsuario { get; set; }
        public string EstadoVendedor { get; set; } = null!;
        public DateTime FechaRegistro { get; set; }

        public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
        public virtual ICollection<Tienda> Tienda { get; set; } = new List<Tienda>();
    }
}