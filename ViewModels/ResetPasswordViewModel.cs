using System.ComponentModel.DataAnnotations;

namespace ECOMMERCE_NEXOSOFT.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener mínimo 8 caracteres")]
        public string NuevaContrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes confirmar la contraseña")]
        [DataType(DataType.Password)]
        [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}