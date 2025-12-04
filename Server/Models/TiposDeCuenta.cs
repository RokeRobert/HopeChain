using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.TiposDeCuenta
    [Table("TiposDeCuenta")]
    public class TipoDeCuenta
    {
        [Key]
        [Column("TipoCuentaID")]
        public int Id { get; set; }

        [Column("NombreTipoDeCuenta")]
        public string NombreTipoDeCuenta { get; set; } = string.Empty;
    }
}