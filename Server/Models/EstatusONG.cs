using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Renombrado de dbo.Estatus a dbo.Estatus_ONG
    [Table("Estatus_ONG")]
    public class EstatusONG
    {
        [Key]
        [Column("EstatusID")]
        public int Id { get; set; }

        [Column("NombreEstatus")]
        public string NombreEstatus { get; set; } = string.Empty;
    }
}