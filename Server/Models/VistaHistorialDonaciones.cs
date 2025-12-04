using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la Función de Tabla: Vista_HistorialDonaciones_Usuario
    public class VistaHistorialDonaciones
    {
        [Key] // Clave compuesta temporal
        [Column("Recibo_Economico_ID")]
        public int ReciboId { get; set; }
        
        public DateTime Fecha { get; set; }
        
        public decimal Monto { get; set; }
        
        [Column("Nombre_ONG")]
        public string NombreONG { get; set; } = string.Empty;
    }
}