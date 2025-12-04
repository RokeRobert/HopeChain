using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransparencyServer.Data;
using Microsoft.Data.SqlClient;
using TransparencyServer.Models;

namespace TransparencyServer.Controllers
{
    [Route("api/comentarios")]
    [ApiController]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ComentariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. OBTENER COMENTARIOS PLATAFORMA (GET)
        [HttpGet("plataforma")]
        public async Task<IActionResult> GetComentariosPlataforma()
        {
            try
            {
                // CORRECCIÓN: Agregamos 'C.Fecha' al SELECT para que coincida con el DTO
                var query = @"
                    SELECT TOP 6 
                        C.Comentario AS Texto, 
                        C.Valoracion, 
                        U.Nombres + ' ' + U.ApellidoPaterno AS Autor,
                        C.Fecha  
                    FROM Comentarios C
                    JOIN Usuarios U ON C.UsuarioID = U.UsuarioID
                    ORDER BY C.Fecha DESC"; 

                var comentarios = await _context.Database
                    .SqlQueryRaw<ComentarioVistaDto>(query)
                    .ToListAsync();

                return Ok(comentarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // 2. PUBLICAR COMENTARIO (POST)
        [HttpPost("plataforma")]
        public async Task<IActionResult> PostComentario([FromBody] NuevoComentarioDto req)
        {
            try
            {
                var pUsuario = new SqlParameter("@UsuarioID", req.UsuarioId);
                var pTexto = new SqlParameter("@Comentario", req.Texto);
                var pValor = new SqlParameter("@Valoracion", req.Valoracion);

                // Llamamos a tu SP existente
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_Insertar_Comentario @UsuarioID, @Comentario, @Valoracion", 
                    pUsuario, pTexto, pValor);

                return Ok(new { success = true, message = "Comentario publicado." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al guardar: " + ex.Message });
            }
        }


        


    }

    
}