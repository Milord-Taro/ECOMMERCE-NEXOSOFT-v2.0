using System.ComponentModel.DataAnnotations;
using ECOMMERCE_NEXOSOFT.Helpers;

namespace ECOMMERCE_NEXOSOFT.ViewModels
{
    public class SolicitudVendedorViewModel
    {
        [Required(ErrorMessage = "El nombre de la tienda es obligatorio.")]
        [RegularExpression(ValidationRules.StoreNamePattern, ErrorMessage = "El nombre de la tienda debe tener entre 2 y 30 caracteres y solo puede contener letras, números, espacios, &, - y .")]
        public string NombreTiendaSolicitada { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción de la tienda es obligatoria.")]
        [StringLength(150, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 150 caracteres.")]
        public string DescripcionTienda { get; set; } = string.Empty;

        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "La razón social debe tener entre 3 y 150 caracteres.")]
        public string RazonSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El NIT es obligatorio.")]
        [RegularExpression(ValidationRules.NitPattern, ErrorMessage = "El NIT debe tener entre 9 y 10 dígitos numéricos.")]
        public string NitRut { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del representante es obligatorio.")]
        [RegularExpression(ValidationRules.NamePattern, ErrorMessage = "El nombre del representante debe tener entre 3 y 50 caracteres y solo puede contener letras, espacios simples o guion.")]
        public string NombreRepresentante { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono de contacto es obligatorio.")]
        [RegularExpression(ValidationRules.PhonePattern, ErrorMessage = "El teléfono debe tener 10 dígitos y comenzar por 3 o 6.")]
        public string TelefonoContacto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo de contacto es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo de contacto no tiene un formato válido.")]
        [StringLength(100, ErrorMessage = "El correo de contacto no puede superar 100 caracteres.")]
        public string CorreoContacto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección comercial es obligatoria.")]
        [RegularExpression(ValidationRules.AddressPattern, ErrorMessage = "La dirección comercial debe tener entre 10 y 150 caracteres y solo puede contener letras, números, espacios y símbolos permitidos (# - . , /).")]
        public string DireccionComercial { get; set; } = string.Empty;
    }
}