using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransparencyServer.Models
{
    // Mapea a la tabla dbo.Usuarios
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [Column("UsuarioID")]
        public int Id { get; set; }

        public string Nombres { get; set; } = string.Empty;
        
        [Column("ApellidoPaterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Column("ApellidoMaterno")]
        public string ApellidoMaterno { get; set; } = string.Empty;
        
        [Column("CorreoElectronico")]
        public string CorreoElectronico { get; set; } = string.Empty;
        
        // CAMBIO CRÍTICO V1.3: Contraseña HASHED (VARBINARY(64) se mapea a byte[])
        public byte[] Contrasena { get; set; } = Array.Empty<byte>();

        // NUEVO V1.3: El valor aleatorio para hashing (UNIQUEIDENTIFIER se mapea a Guid)
        public Guid Salt { get; set; } 

        // Claves Foráneas
        [Column("RolID")]
        public int RolId { get; set; }
        
        [Column("PaisID")]
        public int PaisId { get; set; }
        
        [Column("TipoCuentaID")]
        public int TipoCuentaId { get; set; }
    }
}