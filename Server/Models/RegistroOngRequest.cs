using Microsoft.AspNetCore.Http;

namespace TransparencyServer.Models
{
    public class RegistroOngRequest
    {
        // Datos del Usuario
        public string? uNombre { get; set; }
        
        // --- CAMBIO AQUÍ ---
        public string? uApellidoPaterno { get; set; }
        public string? uApellidoMaterno { get; set; }
        // -------------------

        public string? uCorreo { get; set; }
        public string? uPass { get; set; }
        
        // Modo
        public string? modo { get; set; } 
        public int? ongId { get; set; }

        // Datos de la ONG
        public string? oNombre { get; set; }
        public string? oDesc { get; set; }
        public string? oRFC { get; set; }
        public string? oWeb { get; set; }
        public int? oSector { get; set; }
        public int? oPais { get; set; }

        // Datos Contacto
        public string? cNombre { get; set; }
        public string? cTel { get; set; }
        public string? cCorreo { get; set; }

        // Archivos
        public IFormFile? fileLogo { get; set; }
        public IFormFile? fileComprobante { get; set; }
    }
}