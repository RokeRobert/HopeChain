using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransparencyServer.Data;
using TransparencyServer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Globalization;

namespace TransparencyServer.Controllers
{
    [Route("api/reportes")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DASHBOARD GENERAL (Métricas + Gráficas)
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] string periodo)
        {
            try
            {
                DateTime fechaInicio = periodo == "trimestral" ? DateTime.Now.AddMonths(-3) : DateTime.Now.AddYears(-1);

                // --- A. Métricas Generales ---
                
                // 1. Recibos Directos (Tabla Recibo_Donativo_Economico)
                // Nota: Usamos 'FechaDonacion' y 'OngId'
                var recibosQuery = _context.RecibosDonativoEconomico.Where(r => r.FechaDonacion >= fechaInicio);
                decimal directo = await recibosQuery.SumAsync(r => (decimal?)r.Monto) ?? 0;
                int countDirecto = await recibosQuery.CountAsync();

                // 2. Recibos Campañas (Tabla Recibo_Donativo_EconomicoCampaña)
                // Nota: Usamos 'Fecha' y 'CampanaId'
                var recibosCampanaQuery = _context.RecibosDonativoCampana.Where(r => r.Fecha >= fechaInicio);
                decimal campanas = await recibosCampanaQuery.SumAsync(r => (decimal?)r.Monto) ?? 0;
                int countCampana = await recibosCampanaQuery.CountAsync();

                decimal totalDonado = directo + campanas;
                int totalTransacciones = countDirecto + countCampana;

                // 3. ONGs Activas
                int ongsActivas = await _context.Ongs.CountAsync(o => o.EstatusId == 1); 

                // --- B. Datos por Sector (Lógica Segura sin Navigations) ---
                var sectores = await _context.TiposOng.ToListAsync();
                var listaSectores = new List<SectorStatDto>();

                foreach (var sec in sectores)
                {
                    // 1. Buscar IDs de ONGs de este sector
                    var ongsIds = await _context.Ongs
                        .Where(o => o.TipoOngId == sec.Id) 
                        .Select(o => o.Id)
                        .ToListAsync();

                    if (ongsIds.Any())
                    {
                        // 2. Sumar donativos DIRECTOS a esas ONGs
                        var sumaDirecta = await _context.RecibosDonativoEconomico
                            .Where(r => ongsIds.Contains(r.OngId)) 
                            .SumAsync(r => (decimal?)r.Monto) ?? 0;

                        // 3. Sumar donativos a CAMPAÑAS de esas ONGs
                        // Paso A: Obtener IDs de campañas
                        var idsCampanas = await _context.Campanas
                            .Where(c => ongsIds.Contains(c.OngId)) 
                            .Select(c => c.Id)
                            .ToListAsync();

                        decimal sumaCampanas = 0;
                        // Paso B: Sumar recibos de esas campañas
                        if (idsCampanas.Any())
                        {
                            sumaCampanas = await _context.RecibosDonativoCampana
                                .Where(r => idsCampanas.Contains(r.CampanaId)) 
                                .SumAsync(r => (decimal?)r.Monto) ?? 0;
                        }

                        // Agregar al reporte si hay dinero
                        if ((sumaDirecta + sumaCampanas) > 0)
                        {
                            listaSectores.Add(new SectorStatDto { 
                                Sector = sec.NombreTipoONG, 
                                Monto = sumaDirecta + sumaCampanas 
                            });
                        }
                    }
                }

                // --- C. Datos de Tendencia (Línea) - Últimos 6 meses ---
                var flujoMensual = new List<object>();
                for (int i = 5; i >= 0; i--)
                {
                    var mesActual = DateTime.Now.AddMonths(-i);
                    var inicioMes = new DateTime(mesActual.Year, mesActual.Month, 1);
                    var finMes = inicioMes.AddMonths(1).AddDays(-1);

                    var sumaMesDirecto = await _context.RecibosDonativoEconomico
                        .Where(r => r.FechaDonacion >= inicioMes && r.FechaDonacion <= finMes)
                        .SumAsync(r => (decimal?)r.Monto) ?? 0;

                    var sumaMesCampana = await _context.RecibosDonativoCampana
                        .Where(r => r.Fecha >= inicioMes && r.Fecha <= finMes)
                        .SumAsync(r => (decimal?)r.Monto) ?? 0;

                    flujoMensual.Add(new { 
                        mes = mesActual.ToString("MMM", new CultureInfo("es-ES")), 
                        monto = sumaMesDirecto + sumaMesCampana 
                    });
                }

                return Ok(new {
                    totalDonadoGlobal = totalDonado,
                    ongsActivas = ongsActivas,
                    totalTransacciones = totalTransacciones,
                    // Desglose para la tarjeta de métricas
                    transaccionesDirectas = countDirecto,
                    transaccionesCampanas = countCampana,
                    sectorDistribution = listaSectores,
                    flujoTendencia = flujoMensual
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error dashboard: " + ex.Message });
            }
        }

        // 2. TABLA DESGLOSE FONDOS
        [HttpGet("desglose-fondos")]
        public async Task<IActionResult> GetDesgloseFondos()
        {
            try
            {
                var ongs = await _context.Ongs
                    .Where(o => o.EstatusId == 1)
                    .Select(o => new { o.Id, o.Nombre })
                    .ToListAsync();
                
                var resultado = new List<ReporteTopOngDto>();

                foreach (var ong in ongs)
                {
                    // 1. Directo
                    decimal directo = await _context.RecibosDonativoEconomico
                        .Where(r => r.OngId == ong.Id)
                        .SumAsync(r => (decimal?)r.Monto) ?? 0;

                    // 2. Campañas
                    var idsCampanas = await _context.Campanas
                        .Where(c => c.OngId == ong.Id)
                        .Select(c => c.Id)
                        .ToListAsync();

                    decimal campanas = 0;
                    if (idsCampanas.Any())
                    {
                        campanas = await _context.RecibosDonativoCampana
                            .Where(r => idsCampanas.Contains(r.CampanaId))
                            .SumAsync(r => (decimal?)r.Monto) ?? 0;
                    }

                    decimal total = directo + campanas;

                    if (total > 0)
                    {
                        resultado.Add(new ReporteTopOngDto
                        {
                            Id = ong.Id,
                            Nombre = ong.Nombre,
                            TotalGeneral = total,
                            DonadoDirecto = directo,
                            DonadoCampanas = campanas,
                            PorcentajeDirecto = (int)((directo / total) * 100),
                            PorcentajeCampanas = (int)((campanas / total) * 100)
                        });
                    }
                }

                return Ok(resultado.OrderByDescending(x => x.TotalGeneral).Take(10));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error tabla: " + ex.Message });
            }
        }

        // 3. HISTORIAL POR ONG (Para el botón Ver y PDF)
        [HttpGet("historial-ong/{id}")]
        public async Task<IActionResult> GetHistorialOng(int id)
        {
            try
            {
                var ong = await _context.Ongs.FindAsync(id);
                if (ong == null) return NotFound("ONG no encontrada");

                // A. Donaciones Directas
                var directas = await _context.RecibosDonativoEconomico
                    .Where(r => r.OngId == id)
                    .Select(r => new {
                        Fecha = r.FechaDonacion,
                        Monto = r.Monto,
                        Concepto = "Donativo General",
                        Tipo = "Directo"
                    })
                    .ToListAsync();

                // B. Donaciones a Campañas
                var idsCampanas = await _context.Campanas.Where(c => c.OngId == id).Select(c => c.Id).ToListAsync();
                
                var campanas = await _context.RecibosDonativoCampana
                    .Where(r => idsCampanas.Contains(r.CampanaId))
                    .Select(r => new {
                        Fecha = r.Fecha,
                        Monto = r.Monto,
                        Concepto = "Campaña #" + r.CampanaId, 
                        Tipo = "Campaña"
                    })
                    .ToListAsync();

                // Unimos y ordenamos
                var historial = directas.Concat(campanas).OrderByDescending(x => x.Fecha).ToList();

                return Ok(new { nombreOng = ong.Nombre, historial = historial });
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }

    // --- DTOs NECESARIOS ---
    public class ReporteTopOngDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal TotalGeneral { get; set; }
        public decimal DonadoDirecto { get; set; }
        public decimal DonadoCampanas { get; set; }
        public int PorcentajeDirecto { get; set; } 
        public int PorcentajeCampanas { get; set; }
    }

    public class SectorStatDto
    {
        public string Sector { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}