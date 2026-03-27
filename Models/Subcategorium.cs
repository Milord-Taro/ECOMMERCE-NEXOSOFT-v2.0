using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECOMMERCE_NEXOSOFT.Models
{
    public partial class Subcategorium
    {
        public int IdSubcategoria { get; set; }
        [Required]
        public int CodSubcategoria { get; set; }

        [Required]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El nombre de la subcategoría es obligatorio")]
        public string NombreSubcategoria { get; set; } = null!;
        public string? Descripcion { get; set; }
        public bool VisiblePublico { get; set; }

        public virtual Categorium IdCategoriaNavigation { get; set; } = null!;
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}