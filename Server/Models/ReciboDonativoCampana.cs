using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.Recibo_Donativo_EconomicoCampaña
    [Table("Recibo_Donativo_EconomicoCampaña")]
    public class ReciboDonativoCampana
    {
        [Key]
        [Column("Recibo_Economico_ID")]
        public int Id { get; set; }

        [Column("UsuarioID")]
        public int UsuarioId { get; set; }
        
        [Column("Monto", TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        [Column("MonedaID")]
        public int MonedaId { get; set; }
        
        [Column("MetodoPagoID")]
        public int MetodoPagoId { get; set; }
        
        [Column("Fecha")] 
        public DateTime Fecha { get; set; }

        [Column("CampanaID")]
        public int CampanaId { get; set; }
    }
}