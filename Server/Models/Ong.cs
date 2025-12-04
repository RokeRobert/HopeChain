using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    [Table("ONGs")]
    public class Ong
    {
        [Key]
        [Column("ONGID")]
        public int Id { get; set; }

        public string Logo { get; set; } = string.Empty; // Nuevo en V1.3

        [Column("Nombre_ONG")]
        public string Nombre { get; set; } = string.Empty;
        
        public string Descripcion { get; set; } = string.Empty;
        
        [Column("TipoONGID")]
        public int TipoOngId { get; set; } 
        
        [Column("PaisID")]
        public int PaisId { get; set; } 

        [Column("ContactoID")]
        public int ContactoId { get; set; } 

        // --- NUEVOS CAMPOS V1.3 ---
        public string RFC { get; set; } = string.Empty;
        public string Comprobante { get; set; } = string.Empty;
        
        [Column("EstatusID")]
        public int EstatusId { get; set; } 

        public string PlataformaWeb { get; set; } = string.Empty;

        // --- PROPIEDADES EXTRAS PARA REPORTES (SOLUCIÓN A TU ERROR) ---
        // Usamos [NotMapped] para que EF Core sepa que no son columnas reales de la tabla ONGs,
        // sino datos que llenaremos nosotros manualmente en el servidor.
        
        [NotMapped] 
        public decimal TotalRecaudado { get; set; } = 0.00m;
        
        [NotMapped] 
        public decimal GastoDirectoPorcentaje { get; set; } = 0.00m; // <--- ESTA PROPIEDAD FALTABA
    }
}