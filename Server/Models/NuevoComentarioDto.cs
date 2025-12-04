namespace TransparencyServer.Models
{
    public class NuevoComentarioDto
    {
        public int UsuarioId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public int Valoracion { get; set; }
    }
}