using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransparencyServer.Data;
using TransparencyServer.Models;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.Data.SqlClient;

namespace TransparencyServer.Controllers
{
    [Route("api/[controller]")] // Esto crea la ruta: /api/RegistroOng
    [ApiController]
    public class RegistroOngController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Inyectamos la BD y el entorno (para saber dónde guardar archivos)
        public RegistroOngController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost]
public async Task<IActionResult> Registrar([FromForm] RegistroOngRequest request)
{
    try
    {
        // -----------------------------------------------------------
        // 1. GESTIÓN DE CARPETAS Y ARCHIVOS
        // -----------------------------------------------------------
        string? rutaLogoDb = null;
        string? rutaComprobanteDb = null;

        // Ruta base: wwwroot
        string webRootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        // Rutas físicas
        string carpetaLogos = Path.Combine(webRootPath, "logos_ong");
        string carpetaDocs = Path.Combine(webRootPath, "comprobantes_ong");

        // Asegurar que existan las carpetas
        if (!Directory.Exists(carpetaLogos)) Directory.CreateDirectory(carpetaLogos);
        if (!Directory.Exists(carpetaDocs)) Directory.CreateDirectory(carpetaDocs);
        
        // --- GUARDAR LOGO ---
        if (request.fileLogo != null)
        {
            // Generar nombre único: GUID + extensión original (.png, .jpg)
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.fileLogo.FileName);
            string filePath = Path.Combine(carpetaLogos, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.fileLogo.CopyToAsync(stream);
            }
            // Ruta relativa para la BD (ej: "logos_ong/mi_imagen.png")
            rutaLogoDb = "logos_ong/" + fileName; 
        }

        // --- GUARDAR COMPROBANTE ---
        if (request.fileComprobante != null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.fileComprobante.FileName);
            string filePath = Path.Combine(carpetaDocs, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.fileComprobante.CopyToAsync(stream);
            }
            // Ruta relativa para la BD
            rutaComprobanteDb = "comprobantes_ong/" + fileName;
        }

        // -----------------------------------------------------------
        // 2. PREPARAR DATOS USUARIO
        // -----------------------------------------------------------
        string apellidoP = request.uApellidoPaterno ?? "";
        string apellidoM = request.uApellidoMaterno ?? "";

        bool esNueva = request.modo == "nueva";

        // -----------------------------------------------------------
        // 3. LLAMADA A BASE DE DATOS (CON EL NUEVO LOGO)
        // -----------------------------------------------------------
        var query = @"EXEC sp_Registrar_Usuario_Y_ONG_Completo 
                        @NombreUser, @ApellidoP, @ApellidoM, @CorreoUser, @PassUser,
                        @EsNuevaONG, @ONGID_Existente,
                        @NombreONG, @DescripcionONG, @TipoONGID, @PaisID, @RFC, 
                        @RutaComprobante, @PlataformaWeb, @LogoPath,
                        @NombreContacto, @CorreoContacto, @TelContacto";

        var parametros = new[]
        {
            // Usuario
            new SqlParameter("@NombreUser", request.uNombre ?? ""),
            new SqlParameter("@ApellidoP", apellidoP),
            new SqlParameter("@ApellidoM", apellidoM),
            new SqlParameter("@CorreoUser", request.uCorreo ?? ""),
            new SqlParameter("@PassUser", request.uPass ?? ""),
            
            // Lógica
            new SqlParameter("@EsNuevaONG", esNueva),
            new SqlParameter("@ONGID_Existente", (object?)request.ongId ?? DBNull.Value),

            // ONG Nueva
            new SqlParameter("@NombreONG", (object?)request.oNombre ?? DBNull.Value),
            new SqlParameter("@DescripcionONG", (object?)request.oDesc ?? DBNull.Value),
            new SqlParameter("@TipoONGID", (object?)request.oSector ?? 1), 
            new SqlParameter("@PaisID", (object?)request.oPais ?? 1),       
            new SqlParameter("@RFC", (object?)request.oRFC ?? DBNull.Value),
            new SqlParameter("@RutaComprobante", (object?)rutaComprobanteDb ?? DBNull.Value),
            new SqlParameter("@PlataformaWeb", (object?)request.oWeb ?? DBNull.Value),
            
            // ---> AQUÍ PASAMOS LA RUTA DEL LOGO <---
            new SqlParameter("@LogoPath", (object?)rutaLogoDb ?? DBNull.Value), 
            
            // Contacto
            new SqlParameter("@NombreContacto", (object?)request.cNombre ?? DBNull.Value),
            new SqlParameter("@CorreoContacto", (object?)request.cCorreo ?? DBNull.Value),
            new SqlParameter("@TelContacto", (object?)request.cTel ?? DBNull.Value)
        };

        var resultado = await _context.SpRegistroResponses
                            .FromSqlRaw(query, parametros)
                            .ToListAsync();

        var respuesta = resultado.FirstOrDefault();

        if (respuesta != null && respuesta.Resultado == 1)
        {
            return Ok(new { success = true, message = respuesta.Mensaje });
        }
        else
        {
            return BadRequest(new { success = false, message = respuesta?.Mensaje ?? "Error al registrar en BD" });
        }
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { success = false, message = "Error interno: " + ex.Message });
    }
}
    }
}