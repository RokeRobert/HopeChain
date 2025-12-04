namespace TransparencyServer.Models
{
    // Clase para la tabla de Top ONGs
    public class ReporteTopOngDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal TotalGeneral { get; set; }
        public decimal DonadoDirecto { get; set; }
        public decimal DonadoCampanas { get; set; }
        public int PorcentajeDirecto { get; set; } 
        public int PorcentajeCampanas { get; set; }
    }

    // Clase para la gráfica de pastel
    public class SectorStatDto
    {
        public string Sector { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}