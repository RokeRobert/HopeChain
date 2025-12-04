namespace TransparencyServer.Models
{
    public class DonacionRequest
    {
        public decimal Monto { get; set; }
        
        // Puede ser null si permites donaciones anónimas (aunque el frontend lo bloquee)
        public int? UsuarioId { get; set; }
        
        public string Titular { get; set; } = string.Empty;

        // --- ESTA ES LA PROPIEDAD QUE FALTABA ---
        // Sirve para saber si el dinero va a una ONG específica o a la plataforma (999)
        public int? OngId { get; set; } 
    }
}