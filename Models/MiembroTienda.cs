namespace ECOMMERCE_NEXOSOFT.Models
{
    public partial class MiembroTienda
    {
        public int IdMiembroTienda { get; set; }

        public int IdUsuario { get; set; }

        public int IdTienda { get; set; }

        public int IdRolTienda { get; set; }

        public DateTime FechaIngreso { get; set; }

        public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

        public virtual Tienda IdTiendaNavigation { get; set; } = null!;

        public virtual RolTienda IdRolTiendaNavigation { get; set; } = null!;
    }
}