namespace TransparencyServer.Models
{
    public class RegistroUsuarioDto
    {
        public string Nombres { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public int PaisId { get; set; }
        public int TipoCuentaId { get; set; }
        // RolID se asignará automáticamente como 1 (Donante) por defecto en el backend
    }
}