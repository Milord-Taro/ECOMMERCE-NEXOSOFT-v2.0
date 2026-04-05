using System.ComponentModel.DataAnnotations;
using ECOMMERCE_NEXOSOFT.Helpers;

namespace ECOMMERCE_NEXOSOFT.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [RegularExpression(ValidationRules.PasswordPattern, ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, incluir mayúscula, minúscula, número, carácter especial y no contener espacios.")]
        public string NuevaContrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes confirmar la contraseña.")]
        [DataType(DataType.Password)]
        [Compare("NuevaContrasena", ErrorMessage = "La confirmación de contraseña no coincide.")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}