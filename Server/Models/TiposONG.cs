using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.Tipos_ONG
    [Table("Tipos_ONG")]
    public class TipoOng
    {
        [Key]
        [Column("Tipo_ONGID")]
        public int Id { get; set; }

        [Column("NombreTipoONG")]
        public string NombreTipoONG { get; set; } = string.Empty;
    }
}