using System.ComponentModel.DataAnnotations;

namespace ECOMMERCE_NEXOSOFT.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string CorreoElectronico { get; set; }

        [Required]
        public string Contrasena { get; set; }
    }
}