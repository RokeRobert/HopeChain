using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransparencyServer.Data;
using TransparencyServer.Models;

namespace TransparencyServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LISTAR ONGS PENDIENTES (EstatusID = 2)
        [HttpGet("pendientes")]
        public async Task<IActionResult> GetPendientes()
        {
            try
            {
                var pendientes = await _context.Ongs
                    .Where(o => o.EstatusId == 2) // 2 = Pendiente
                    .Select(o => new
                    {
                        o.Id,
                        o.Nombre,
                        o.Descripcion,
                        o.RFC,
                        o.Logo,
                        o.Comprobante, // El PDF
                        FechaRegistro = DateTime.Now // Si tuvieras fecha en BD, úsala aquí
                    })
                    .ToListAsync();

                return Ok(pendientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno: " + ex.Message });
            }
        }

        // 2. APROBAR O RECHAZAR (Cambiar Estatus)
        [HttpPost("cambiar-estatus")]
        public async Task<IActionResult> CambiarEstatus([FromBody] EstatusRequest request)
        {
            try
            {
                var ong = await _context.Ongs.FindAsync(request.OngId);
                if (ong == null) return NotFound(new { message = "ONG no encontrada" });

                // Actualizamos el estatus (1=Activa, 3=Rechazada)
                ong.EstatusId = request.NuevoEstatus;
                
                await _context.SaveChangesAsync();

                string accion = request.NuevoEstatus == 1 ? "Aprobada" : "Rechazada";
                return Ok(new { success = true, message = $"La ONG ha sido {accion}." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar: " + ex.Message });
            }
        }
    }

    // Clase pequeña para recibir los datos del cambio
    public class EstatusRequest
    {
        public int OngId { get; set; }
        public int NuevoEstatus { get; set; }
    }
}