using System.ComponentModel.DataAnnotations;
using ECOMMERCE_NEXOSOFT.Helpers;

namespace ECOMMERCE_NEXOSOFT.ViewModels
{
    public class PerfilViewModel
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(ValidationRules.NamePattern, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres y solo puede contener letras, espacios simples o guion.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [RegularExpression(ValidationRules.NamePattern, ErrorMessage = "El apellido debe tener entre 3 y 50 caracteres y solo puede contener letras, espacios simples o guion.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(ValidationRules.PhonePattern, ErrorMessage = "El teléfono debe tener 10 dígitos y comenzar por 3 o 6.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(100, ErrorMessage = "El correo no puede superar 100 caracteres.")]
        public string CorreoElectronico { get; set; } = string.Empty;

        public int IdRol { get; set; }

        [RegularExpression(ValidationRules.AddressPattern, ErrorMessage = "La dirección debe tener entre 10 y 150 caracteres y solo puede contener letras, números, espacios y símbolos permitidos (# - . , /).")]
        public string? Direccion1 { get; set; }

        [RegularExpression(ValidationRules.AddressPattern, ErrorMessage = "La dirección debe tener entre 10 y 150 caracteres y solo puede contener letras, números, espacios y símbolos permitidos (# - . , /).")]
        public string? Direccion2 { get; set; }

        [RegularExpression(ValidationRules.AddressPattern, ErrorMessage = "La dirección debe tener entre 10 y 150 caracteres y solo puede contener letras, números, espacios y símbolos permitidos (# - . , /).")]
        public string? Direccion3 { get; set; }

        public bool EsCuentaInternaTienda { get; set; }

        [Required(ErrorMessage = "El tipo de identificación es obligatorio.")]
        public string? TipoIdentificacion { get; set; }

        [Required(ErrorMessage = "El número de identificación es obligatorio.")]
        public string? NumeroIdentificacion { get; set; }

        [RegularExpression(ValidationRules.PasswordPattern, ErrorMessage = "La nueva contraseña debe tener mínimo 8 caracteres, incluir mayúscula, minúscula, número, carácter especial y no contener espacios.")]
        public string? NuevaContrasena { get; set; }

        [Compare("NuevaContrasena", ErrorMessage = "La confirmación de contraseña no coincide.")]
        public string? ConfirmarNuevaContrasena { get; set; }
    }
}