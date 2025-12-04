using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.Rol
    [Table("Rol")]
    public class Rol
    {
        [Key]
        [Column("RolID")] // CORREGIDO de RollD a RolID
        public int Id { get; set; }

        [Column("NombreRol")]
        public string NombreRol { get; set; } = string.Empty;
    }
}