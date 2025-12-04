using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.Usuarios_ONGs
    [Table("Usuarios_ONGs")]
    public class UsuarioONG
    {
        [Key]
        [Column("Usuario_ONGID")]
        public int Id { get; set; }
        
        public string Nombre { get; set; } = string.Empty;
        
        [Column("ApellidoPaterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Column("ApellidoMaterno")]
        public string ApellidoMaterno { get; set; } = string.Empty;
        
        [Column("CorreoElectronico")]
        public string CorreoElectronico { get; set; } = string.Empty;
        
        // Contraseña HASHED (VARBINARY(64) se mapea a byte[])
        public byte[] Contrasena { get; set; } = Array.Empty<byte>();
        // Salt (UNIQUEIDENTIFIER se mapea a Guid)
        public Guid Salt { get; set; }
        
        [Column("ONGID")]
        public int OngId { get; set; }
    }
}