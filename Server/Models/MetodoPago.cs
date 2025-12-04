using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.MetodoPago
    [Table("MetodoPago")]
    public class MetodoPago
    {
        [Key]
        [Column("MetodoPagoID")]
        public int Id { get; set; }

        [Column("NombreMetodoPago")]
        public string NombreMetodoPago { get; set; } = string.Empty;
    }
}