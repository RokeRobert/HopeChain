using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.Recibo_Donativo_Economico
    [Table("Recibo_Donativo_Economico")]
    public class ReciboDonativo
    {
        [Key]
        [Column("Recibo_Economico_ID")]
        public int Id { get; set; }

        [Column("UsuarioID")]
        public int UsuarioId { get; set; }
        
        [Column("Monto", TypeName = "decimal(18, 2)")] // Mapeo exacto del tipo decimal
        public decimal Monto { get; set; }

        [Column("MonedaID")]
        public int MonedaId { get; set; }
        
        [Column("MetodoPagoID")]
        public int MetodoPagoId { get; set; }
        
        [Column("Fecha")] // El tipo 'date' en SQL se mapea a DateTime en C#
        public DateTime FechaDonacion { get; set; }

        // Clave Foránea a la ONG
        [Column("ONGID")]
        public int OngId { get; set; }

        // Propiedades de navegación (opcional)
        // public virtual Usuario Usuario { get; set; } = default!;
        // public virtual Moneda Moneda { get; set; } = default!;
        // public virtual MetodoPago MetodoPago { get; set; } = default!;
        // public virtual Ong Ong { get; set; } = default!;
    }
}