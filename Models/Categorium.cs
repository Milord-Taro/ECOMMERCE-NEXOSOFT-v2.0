using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ECOMMERCE_NEXOSOFT.Helpers;

namespace ECOMMERCE_NEXOSOFT.Models
{
    public partial class Categorium
    {
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El código de la categoría es obligatorio.")]
        public int CodCategoria { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "El nombre de la categoría debe tener entre 3 y 40 caracteres.")]
        [RegularExpression(ValidationRules.CategoryOrSubcategoryNamePattern, ErrorMessage = "El nombre de la categoría no puede estar vacío ni contener solo símbolos.")]
        public string NombreCategoria { get; set; } = null!;

        [Required(ErrorMessage = "La descripción de la categoría es obligatoria.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "La descripción de la categoría debe tener entre 3 y 100 caracteres.")]
        [RegularExpression(ValidationRules.CategoryDescriptionPattern, ErrorMessage = "La descripción de la categoría no puede estar vacía ni contener solo símbolos.")]
        public string? Descripcion { get; set; }

        public bool VisiblePublico { get; set; }

        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();

        public virtual ICollection<Subcategorium> Subcategoria { get; set; } = new List<Subcategorium>();
    }
}