using System.ComponentModel.DataAnnotations;

namespace ECOMMERCE_NEXOSOFT.ViewModels
{
    public class MiembroTiendaEditViewModel
    {
        public int IdMiembroTienda { get; set; }
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede superar 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de identificación es obligatorio.")]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de identificación es obligatorio.")]
        [RegularExpression(@"^\d{6,10}$", ErrorMessage = "El número de identificación debe contener solo números y entre 6 y 10 dígitos.")]
        public string NumeroIdentificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe contener solo números y 10 dígitos empezando por 3.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        [RegularExpression(@"^[^0-9].*", ErrorMessage = "El correo no puede iniciar con un número")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes seleccionar un rol interno.")]
        public int IdRolTienda { get; set; }

        [StringLength(100, MinimumLength = 8, ErrorMessage = "La nueva contraseña debe tener entre 8 y 100 caracteres.")]
        public string? NuevaContrasena { get; set; }

        [Compare("NuevaContrasena", ErrorMessage = "La confirmación de contraseña no coincide.")]
        public string? ConfirmarNuevaContrasena { get; set; }
    }
}