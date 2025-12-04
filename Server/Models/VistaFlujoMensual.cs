using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la Función de Tabla: Vista_Flujo_Donaciones_Economicas_Mensual
    public class VistaFlujoMensual
    {
        [Key] // Clave temporal para EF Core
        [Column("Numero_Mes")]
        public int NumeroMes { get; set; }

        [Column("Mes_Nombre")]
        public string NombreMes { get; set; } = string.Empty;
        
        [Column("MontoTotal")]
        public decimal MontoTotal { get; set; }
    }
}