using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.Contacto
    [Table("Contacto")]
    public class Contacto
    {
        [Key]
        [Column("ContactoID")]
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        [Column("CorreoElectronico")]
        public string CorreoElectronico { get; set; } = string.Empty;

        public string? Telefono { get; set; } // Puede ser nulo en SQL, por eso se usa '?'
    }
}