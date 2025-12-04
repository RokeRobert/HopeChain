using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.Paises
    [Table("Paises")]
    public class Pais
    {
        [Key]
        [Column("PaisesID")]
        public int Id { get; set; }

        [Column("NombrePaises")]
        public string Nombre { get; set; } = string.Empty;
    }
}