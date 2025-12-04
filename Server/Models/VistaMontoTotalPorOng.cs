using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models 
{
    // Mapea a la Vista: Vista_MontoTotal_Por_ONG
    public class VistaMontoTotalPorOng
    {
        [Column("Nombre_ONG")] 
        public string NombreONG { get; set; } = string.Empty; 
        
        [Column("TotalRecibido")]
        public decimal TotalRecibido { get; set; }

        // SE ELIMINÓ: [Column("PorcentajeGastoDirecto")] porque ya no se usará.
    }
}