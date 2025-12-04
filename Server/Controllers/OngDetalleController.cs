using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransparencyServer.Data;
using TransparencyServer.Models;
using Microsoft.Data.SqlClient;

namespace TransparencyServer.Controllers
{
    [Route("api/detalle-ong")]
    [ApiController]
    public class OngDetalleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OngDetalleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. OBTENER DETALLE COMPLETO (GET /api/detalle-ong/{id})
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetalle(int id)
        {
            try 
            {
                var ong = await _context.Ongs
                    .Where(o => o.Id == id) // Ojo: Si falla, usa o.ONGID o o.Id según tu modelo
                    .Select(o => new {
                        Id = o.Id,
                        Nombre = o.Nombre, // Ojo: o.Nombre_ONG en SQL
                        Descripcion = o.Descripcion,
                        Logo = o.Logo,
                        PlataformaWeb = o.PlataformaWeb ?? "", // Evitar nulos
                        Pais = "México", 
                        Sector = "General" 
                    })
                    .FirstOrDefaultAsync();

                if (ong == null) return NotFound(new { message = "ONG no encontrada" });

                return Ok(ong);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error detalle: " + ex.Message });
            }
        }

        // 2. OBTENER CAMPAÑAS DE LA ONG (CORREGIDO Y SEGURO)
        [HttpGet("{id}/campanas")]
        public async Task<IActionResult> GetCampanas(int id)
        {
            try
            {
                // PASO A: Traer la información básica de las campañas usando LINQ normal
                // (Sin intentar traer la imagen aquí para evitar el error 500)
                var campanasBase = await _context.Campanas
                    .Where(c => c.OngId == id) // Usamos la propiedad del Modelo C#
                    .Select(c => new {
                        c.Id,      
                        c.Nombre, 
                        c.Descripcion
                    })
                    .ToListAsync();

                // PASO B: Llenar la imagen una por una (Esto es seguro)
                var listaFinal = new List<object>();

                foreach (var c in campanasBase)
                {
                    string? rutaImagen = null;
                    try
                    {
                        // Consulta directa solo para la imagen
                        var resultado = await _context.Database
                            .SqlQueryRaw<string>("SELECT TOP 1 Ruta FROM Imagenes_Campanas WHERE CampanaID = {0}", c.Id)
                            .ToListAsync();
                        
                        rutaImagen = resultado.FirstOrDefault();
                    }
                    catch { /* Si falla la imagen, continuamos sin ella */ }

                    // Agregamos a la lista con los nombres que espera el JavaScript
                    listaFinal.Add(new {
                        campanaID = c.Id,
                        nombreCampana = c.Nombre,
                        descripcion = c.Descripcion,
                        imagen = rutaImagen
                    });
                }

                return Ok(listaFinal);
            }
            catch (Exception ex)
            {
                // Esto te ayudará a ver el error real en la consola de Visual Studio
                Console.WriteLine("ERROR GET CAMPANAS: " + ex.Message);
                return StatusCode(500, new { message = "Error interno: " + ex.Message });
            }
        }

        // 3. OBTENER COMENTARIOS DE LA ONG
        [HttpGet("{id}/comentarios")]
        public async Task<IActionResult> GetComentarios(int id)
        {
            // CORRECCIÓN: Agregamos 'C.Fecha' aquí también
            var query = @"
                SELECT TOP 5 
                    C.Comentario AS Texto, 
                    C.Valoracion, 
                    ISNULL(U.Nombres, 'Anónimo') AS Autor,
                    C.Fecha 
                FROM Comentarios_ONGs C
                LEFT JOIN Usuarios U ON C.UsuarioID = U.UsuarioID
                WHERE C.ONGID = {0}
                ORDER BY C.Fecha DESC";

            var comentarios = await _context.Database
                .SqlQueryRaw<ComentarioVistaDto>(query, id)
                .ToListAsync();

            return Ok(comentarios);
        }

        // 4. PUBLICAR COMENTARIO
        [HttpPost("{id}/comentarios")]
        public async Task<IActionResult> PostComentario(int id, [FromBody] NuevoComentarioDto req)
        {
            try
            {
                var pUser = new SqlParameter("@UsuarioID", req.UsuarioId);
                var pOng = new SqlParameter("@ONGID", id);
                var pTexto = new SqlParameter("@Comentario", req.Texto);
                var pVal = new SqlParameter("@Valoracion", req.Valoracion);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_Insertar_ComentarioONG @UsuarioID, @ONGID, @Comentario, @Valoracion",
                    pUser, pOng, pTexto, pVal);

                return Ok(new { success = true });
            }
            catch (Exception ex) 
            { 
                return StatusCode(500, new { message = ex.Message }); 
            }
        }
    }
}