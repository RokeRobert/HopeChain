using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransparencyServer.Data;
using TransparencyServer.Models;
using System.Collections.Generic; // Necesario para Listas

namespace TransparencyServer.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. ESTADÍSTICAS (GET /api/home/stats)
        [HttpGet("stats")]
public async Task<IActionResult> GetStats()
{
    try
    {
        var totalOngs = await _context.Ongs.CountAsync(o => o.EstatusId == 1);
        var donacionesDirectas = await _context.RecibosDonativoEconomico.CountAsync();
        var donacionesCampanas = await _context.RecibosDonativoCampana.CountAsync();
        var totalPaises = await _context.Ongs.Select(o => o.PaisId).Distinct().CountAsync();
        
        // NUEVO: Contamos las campañas como "Reportes Públicos"
        var totalCampanas = await _context.Campanas.CountAsync();

        return Ok(new {
            ongs = totalOngs,
            donaciones = donacionesDirectas + donacionesCampanas,
            paises = totalPaises,
            reportes = totalCampanas // <--- Ahora es dinámico
        });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Error estadísticas: " + ex.Message });
    }
}

        // 2. CAMPAÑAS DESTACADAS (Versión Final: Con Imágenes y Redirección ONG)
[HttpGet("campanas-destacadas")]
public async Task<IActionResult> GetDestacadas()
{
    try 
    {
        // PASO 1: Traemos los datos base de la campaña
        var campañasBase = await _context.Campanas
            .OrderByDescending(c => c.Id)
            .Take(6)
            .Select(c => new 
            {
                c.Id,
                c.Nombre, 
                c.Descripcion,
                c.OngId // <--- Fundamental para que funcione el botón "Ver detalles"
            })
            .ToListAsync();

        var listaFinal = new List<object>();

        // PASO 2: Buscamos la imagen para cada campaña encontrada
        foreach(var c in campañasBase)
        {
            var rutaImagen = "";
            try {
                // Tu lógica original para obtener la imagen desde la tabla "Imagenes_Campanas"
                var resultado = await _context.Database
                    .SqlQueryRaw<string>("SELECT TOP 1 Ruta FROM Imagenes_Campanas WHERE CampanaID = {0}", c.Id)
                    .ToListAsync();
                
                rutaImagen = resultado.FirstOrDefault();
            } catch { 
                // Si falla la imagen, no detenemos el proceso, se queda vacía
            }

            // PASO 3: Armamos el objeto final para el Frontend
            listaFinal.Add(new {
                id = c.Id,
                ongId = c.OngId, // <--- ¡CORREGIDO! Aquí debe ser c.OngId (no c.Id)
                nombre = c.Nombre,
                descripcion = c.Descripcion,
                imagen = rutaImagen // <--- Aquí va la imagen recuperada
            });
        }

        return Ok(listaFinal);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Error: " + ex.Message });
    }
}
    }
}