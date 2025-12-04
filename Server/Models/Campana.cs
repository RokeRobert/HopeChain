using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.Campañas
    [Table("Campañas")]
    public class Campana
    {
        [Key]
        [Column("CampanaID")]
        public int Id { get; set; }

        [Column("NombreCampana")]
        public string Nombre { get; set; } = string.Empty;

        [Column("ONGID")]
        public int OngId { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public int Valoracion { get; set; }
    }
}