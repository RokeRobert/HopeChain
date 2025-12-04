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

        // 2. CAMPAÑAS DESTACADAS
        [HttpGet("campanas-destacadas")]
        public async Task<IActionResult> GetDestacadas()
        {
            try 
            {
                var campañasBase = await _context.Campanas
                    .OrderByDescending(c => c.Id)
                    .Take(6)
                    .Select(c => new 
                    {
                        c.Id,
                        // Aquí está la clave: Asignamos el nombre de la propiedad
                        c.Nombre, 
                        c.Descripcion
                    })
                    .ToListAsync();

                var listaFinal = new List<object>();

                foreach(var c in campañasBase)
                {
                    var rutaImagen = "";
                    try {
                        var resultado = await _context.Database
                            .SqlQueryRaw<string>("SELECT TOP 1 Ruta FROM Imagenes_Campanas WHERE CampanaID = {0}", c.Id)
                            .ToListAsync();
                        
                        rutaImagen = resultado.FirstOrDefault();
                    } catch { }

                    // ---> AQUÍ DEFINIMOS LOS NOMBRES QUE LEERÁ EL JS <---
                    listaFinal.Add(new {
                        id = c.Id,
                        nombre = c.Nombre,  // JS leerá: camp.nombre
                        descripcion = c.Descripcion, // JS leerá: camp.descripcion
                        imagen = rutaImagen       // JS leerá: camp.imagen
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