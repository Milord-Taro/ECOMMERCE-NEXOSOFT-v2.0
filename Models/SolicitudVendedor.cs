using System;
using System.Collections.Generic;

namespace ECOMMERCE_NEXOSOFT.Models
{
    public partial class SolicitudVendedor
    {
        public int IdSolicitudVendedor { get; set; }

        public int CodSolicitudVendedor { get; set; }

        public int IdUsuario { get; set; }

        public string NombreTiendaSolicitada { get; set; } = null!;

        public string? DescripcionTienda { get; set; }

        public string EstadoSolicitud { get; set; } = null!;

        public string? ObservacionAdmin { get; set; }

        public DateTime FechaSolicitud { get; set; }

        public DateTime? FechaRespuesta { get; set; }

        public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

        public string? RazonSocial { get; set; }

        public string? NitRut { get; set; }

        public string? NombreRepresentante { get; set; }

        public string? TelefonoContacto { get; set; }

        public string? CorreoContacto { get; set; }

        public string? DireccionComercial { get; set; }
    }
}