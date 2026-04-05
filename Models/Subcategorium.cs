using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ECOMMERCE_NEXOSOFT.Helpers;

namespace ECOMMERCE_NEXOSOFT.Models
{
    public partial class Subcategorium
    {
        public int IdSubcategoria { get; set; }

        [Required(ErrorMessage = "El código de la subcategoría es obligatorio.")]
        public int CodSubcategoria { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El nombre de la subcategoría es obligatorio.")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "El nombre de la subcategoría debe tener entre 3 y 40 caracteres.")]
        [RegularExpression(ValidationRules.CategoryOrSubcategoryNamePattern, ErrorMessage = "El nombre de la subcategoría no puede estar vacío ni contener solo símbolos.")]
        public string NombreSubcategoria { get; set; } = null!;

        [Required(ErrorMessage = "La descripción de la subcategoría es obligatoria.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "La descripción de la subcategoría debe tener entre 3 y 150 caracteres.")]
        [RegularExpression(ValidationRules.SubcategoryDescriptionPattern, ErrorMessage = "La descripción de la subcategoría no puede estar vacía ni contener solo símbolos.")]
        public string? Descripcion { get; set; }

        public bool VisiblePublico { get; set; }

        public virtual Categorium IdCategoriaNavigation { get; set; } = null!;

        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}