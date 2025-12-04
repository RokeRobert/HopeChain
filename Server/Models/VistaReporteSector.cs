using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
// Estas librerías son obligatorias para los atributos de mapeo de base de datos.
using Microsoft.EntityFrameworkCore;

// Corregido: Este namespace debe coincidir con el nombre de tu proyecto.
namespace TransparencyServer.Models
{
    // Mapea a la Vista SQL: Vista_Distribucion_Donaciones_Economicas
    public class VistaReporteSector
    {
        // Se usa Key para satisfacer a EF Core, aunque la vista no tenga PK real.
        [Key] 
        [Column("NombreTipoONG")] // Nombre de la columna en la vista SQL
        public string NombreSector { get; set; } = string.Empty; 
        
        [Column("TotalDonado")] // Nombre de la columna en la vista SQL
        public decimal MontoTotal { get; set; }
    }
}