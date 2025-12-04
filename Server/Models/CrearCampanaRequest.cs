using Microsoft.AspNetCore.Http;

namespace TransparencyServer.Models
{
    public class CrearCampanaRequest
    {
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int ongId { get; set; } // El ID de la ONG que publica
        public IFormFile? imagen { get; set; } // El archivo
    }
}