using Microsoft.EntityFrameworkCore; 

namespace TransparencyServer.Models
{
    [Keyless]
    public class LoginResult
    {
        public int ID { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string TipoUsuario { get; set; } = string.Empty;
        public int ID_Rol_o_Entidad { get; set; }
    }
}