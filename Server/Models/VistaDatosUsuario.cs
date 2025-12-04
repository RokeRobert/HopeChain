using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la Función de Tabla: Vista_DatosUsuario
    public class VistaDatosUsuario
    {
        [Key] // Clave temporal para EF Core (puede ser el Correo o el ID)
        public string CorreoElectronico { get; set; } = string.Empty; 

        public string Nombres { get; set; } = string.Empty;
        
        [Column("ApellidoPaterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Column("ApellidoMaterno")]
        public string ApellidoMaterno { get; set; } = string.Empty;
        
        [Column("NombrePaises")]
        public string Pais { get; set; } = string.Empty;

        [Column("NombreRol")]
        public string Rol { get; set; } = string.Empty;
        
        // Campos Calculados
        [Column("TotalDonado")]
        public decimal TotalDonado { get; set; }
        
        [Column("DonacionesRealizadas")]
        public int DonacionesRealizadas { get; set; }
    }
}