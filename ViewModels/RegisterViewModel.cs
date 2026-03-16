using Microsoft.AspNetCore.Mvc;

namespace ECOMMERCE_NEXOSOFT.ViewModels;
    using System.ComponentModel.DataAnnotations;

    public class RegisterViewModel
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }

        [Required]
        public string TipoIdentificacion { get; set; }

        [Required]
        public string NumeroIdentificacion { get; set; }

        [Required]
        public string Telefono { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]

    // ❗ No permitir que empiece por número
    [RegularExpression(@"^[^0-9].*",
 ErrorMessage = "El correo no puede iniciar con un número")]

    public string CorreoElectronico { get; set; }

    [Required]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }

        [Compare("Contrasena")]
        public string ConfirmarContrasena { get; set; }
    }


