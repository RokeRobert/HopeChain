using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransparencyServer.Data;
using TransparencyServer.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace TransparencyServer.Controllers
{
    [Route("api/donaciones")]
    [ApiController]
    public class DonacionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DonacionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // PROCESAR PAGO (POST /api/donaciones/procesar)
        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarDonacion([FromBody] DonacionRequest req)
        {
            try
            {
                if (req.UsuarioId == null || req.UsuarioId <= 0)
                    return Unauthorized(new { success = false, message = "Debes iniciar sesión." });

                // VERIFICACIÓN 1: Asegúrate de que aquí diga 999
                int idPlataforma = 999; 

                // Si req.OngId viene nulo (desde el Home), usa 999.
                // Si viene con dato (desde DetalleONG corregido), usa ese dato.
                int ongDestino = req.OngId ?? idPlataforma; 

                // VERIFICACIÓN 2: Revisa la cadena SQL. 
                // Asegúrate de que el último valor sea @OngId y no un '1' fijo.
                var query = @"INSERT INTO Recibo_Donativo_Economico (UsuarioID, Monto, MonedaID, MetodoPagoID, Fecha, ONGID)
                              VALUES (@Uid, @Monto, 1, 1, GETDATE(), @OngId)"; // <--- AQUÍ

                var parametros = new[] {
                    new SqlParameter("@Uid", req.UsuarioId),
                    new SqlParameter("@Monto", req.Monto),
                    new SqlParameter("@OngId", ongDestino) // <--- Y AQUÍ
                };

                await _context.Database.ExecuteSqlRawAsync(query, parametros);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}