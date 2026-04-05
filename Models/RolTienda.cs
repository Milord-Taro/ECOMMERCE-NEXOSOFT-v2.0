namespace ECOMMERCE_NEXOSOFT.Models
{
    public partial class RolTienda
    {
        public int IdRolTienda { get; set; }

        public string NombreRol { get; set; } = null!;

        public string? Descripcion { get; set; }

        public virtual ICollection<MiembroTienda> Miembros { get; set; } = new List<MiembroTienda>();
    }
}