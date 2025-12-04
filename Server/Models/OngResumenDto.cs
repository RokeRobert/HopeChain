namespace TransparencyServer.Models
{
    public class OngResumenDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? Logo { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public decimal TotalRecaudado { get; set; }
    }
}