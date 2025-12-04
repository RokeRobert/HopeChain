using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransparencyServer.Data;
using TransparencyServer.Models;
using Microsoft.Data.SqlClient;
using System.IO;

namespace TransparencyServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampanasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CampanasController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ---------------------------------------------------------
        // 1. ENDPOINT PARA CREAR CAMPAÑA (POST)
        // ---------------------------------------------------------
        [HttpPost("crear")]
        public async Task<IActionResult> CrearCampana([FromForm] CrearCampanaRequest request)
        {
            try
            {
                // A. Guardar Imagen en carpeta
                string? rutaImagenDb = null;
                string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string carpetaCampanas = Path.Combine(webRootPath, "imagenes_campanas");

                if (!Directory.Exists(carpetaCampanas)) Directory.CreateDirectory(carpetaCampanas);

                if (request.imagen != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.imagen.FileName);
                    string filePath = Path.Combine(carpetaCampanas, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.imagen.CopyToAsync(stream);
                    }
                    rutaImagenDb = "imagenes_campanas/" + fileName;
                }

                // B. Guardar en Base de Datos (SP)
                var pNombre = new SqlParameter("@NombreCampana", request.nombre);
                var pOngId = new SqlParameter("@ONGID", request.ongId);
                var pDesc = new SqlParameter("@Descripcion", request.descripcion);
                var pRuta = new SqlParameter("@RutaImagen", (object?)rutaImagenDb ?? DBNull.Value);

                var resultado = await _context.SpRegistroResponses
                    .FromSqlRaw("EXEC sp_CrearCampanaCompleta @NombreCampana, @ONGID, @Descripcion, @RutaImagen", 
                                pNombre, pOngId, pDesc, pRuta)
                    .ToListAsync();

                var respuesta = resultado.FirstOrDefault();

                if (respuesta != null && respuesta.Resultado == 1)
                {
                    return Ok(new { success = true, message = respuesta.Mensaje });
                }
                else
                {
                    return BadRequest(new { success = false, message = respuesta?.Mensaje ?? "Error en BD" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error interno: " + ex.Message });
            }
        }

        // ---------------------------------------------------------
        // 2. ENDPOINT PARA LISTAR CAMPAÑAS POR ONG (GET) - ¡ESTO FALTABA!
        // ---------------------------------------------------------
        [HttpGet("ong/{ongId}")]
        public async Task<IActionResult> ObtenerCampanasPorOng(int ongId)
        {
            try
            {
                // Consulta directa para traer campañas + su imagen
                var query = @"
                    SELECT 
                        C.CampanaID AS Id,
                        C.NombreCampana AS Nombre,
                        C.Descripcion,
                        (SELECT TOP 1 Ruta FROM Imagenes_Campanas WHERE CampanaID = C.CampanaID) AS Imagen
                    FROM Campañas C
                    WHERE C.ONGID = @OngId
                    ORDER BY C.CampanaID DESC"; 

                var parametros = new[] { new SqlParameter("@OngId", ongId) };

                var campanas = await _context.Database
                    .SqlQueryRaw<CampanaResumenDto>(query, parametros)
                    .ToListAsync();

                return Ok(campanas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener campañas: " + ex.Message });
            }
        }


        // ---------------------------------------------------------
        // 3. ENDPOINT PARA ELIMINAR CAMPAÑA (DELETE)
        // ---------------------------------------------------------
        [HttpDelete("eliminar/{id}")]
        public async Task<IActionResult> EliminarCampaña(int id)
        {
            try
            {
                // Primero borramos la imagen física (Opcional, buena práctica)
                var rutaImagen = await _context.Database
                    .SqlQueryRaw<string>("SELECT Ruta FROM Imagenes_Campanas WHERE CampanaID = {0}", id)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(rutaImagen))
                {
                    string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string pathCompleto = Path.Combine(webRootPath, rutaImagen);
                    if (System.IO.File.Exists(pathCompleto)) System.IO.File.Delete(pathCompleto);
                }

                // Borramos de la BD (Cascada manual o SP)
                // Nota: Si tienes FKs, primero borra imagenes, luego campaña
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Imagenes_Campanas WHERE CampanaID = {0}", id);
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Campañas WHERE CampanaID = {0}", id);

                return Ok(new { success = true, message = "Campaña eliminada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }

        // ---------------------------------------------------------
        // 4. ENDPOINT PARA EDITAR CAMPAÑA (PUT)
        // ---------------------------------------------------------
        // Reutilizamos el modelo CrearCampanaRequest pero añadimos el ID en la ruta
        [HttpPut("editar/{id}")]
        public async Task<IActionResult> EditarCampaña(int id, [FromForm] CrearCampanaRequest request)
        {
            try
            {
                // 1. Actualizar Datos Básicos
                await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Campañas SET NombreCampana = {0}, Descripcion = {1} WHERE CampanaID = {2}",
                    request.nombre, request.descripcion, id);

                // 2. Si viene nueva imagen, la actualizamos
                if (request.imagen != null)
                {
                    // Guardar archivo nuevo
                    string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string carpetaCampanas = Path.Combine(webRootPath, "imagenes_campanas");
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.imagen.FileName);
                    string filePath = Path.Combine(carpetaCampanas, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.imagen.CopyToAsync(stream);
                    }
                    string nuevaRuta = "imagenes_campanas/" + fileName;

                    // Actualizar en BD (Asumiendo 1 imagen por campaña)
                    // Primero verificamos si ya tenía imagen para hacer UPDATE o INSERT
                    var existeImagen = await _context.Database
                        .SqlQueryRaw<int>("SELECT COUNT(*) as Value FROM Imagenes_Campanas WHERE CampanaID = {0}", id)
                        .FirstOrDefaultAsync();

                    if (existeImagen > 0)
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "UPDATE Imagenes_Campanas SET Ruta = {0} WHERE CampanaID = {1}", nuevaRuta, id);
                    }
                    else
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "INSERT INTO Imagenes_Campanas (CampanaID, Ruta) VALUES ({0}, {1})", id, nuevaRuta);
                    }
                }

                return Ok(new { success = true, message = "Campaña actualizada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error al editar: " + ex.Message });
            }
        }


    }
}