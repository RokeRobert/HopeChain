using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.Moneda
    [Table("Moneda")]
    public class Moneda
    {
        [Key]
        [Column("MonedaID")]
        public int Id { get; set; }

        [Column("NombreMoneda")]
        public string NombreMoneda { get; set; } = string.Empty;
    }
}