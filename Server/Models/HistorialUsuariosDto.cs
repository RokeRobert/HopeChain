using System.Collections.Generic;

namespace TransparencyServer.Models // <-- Namespace Correcto
{
    // DTO que combina la información del usuario con su historial de donaciones
    public class HistorialUsuarioDto
    {
        // Datos del Usuario (mapeados desde VistaDatosUsuario)
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        
        // Métricas
        public decimal TotalDonado { get; set; }
        public int DonacionesRealizadas { get; set; }
        
        // Historial de Transacciones (mapeado desde VistaHistorialDonaciones)
        public List<VistaHistorialDonaciones> Historial { get; set; } = new List<VistaHistorialDonaciones>();
    }
}