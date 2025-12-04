namespace TransparencyServer.Models
{
    public class ComentarioVistaDto
    {
        public string Texto { get; set; } = string.Empty;
        public int Valoracion { get; set; }
        public string Autor { get; set; } = string.Empty;
        // Agregamos Fecha por si acaso la necesitas para ordenar
        public DateTime Fecha { get; set; } 
    }
}