using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TransparencyServer.Data; 
using TransparencyServer.Models; 
using TransparencyServer.Services; 
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Data.SqlClient;

var MyAllowClientOrigin = "_myAllowClientOrigin"; 

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE SERVICIOS
// ==========================================

// Base de Datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
    }));

// Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// CORS (Permisos para el navegador)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowClientOrigin,
        policy =>
        {
            policy.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

// ¡VITAL! Habilitar los Controladores (Reportes, Admin, Donaciones)
builder.Services.AddControllers(); 

var app = builder.Build();

// ==========================================
// 2. MIDDLEWARE
// ==========================================

app.UseCors(MyAllowClientOrigin); 
app.UseStaticFiles(); // Para servir imágenes y PDFs

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ¡VITAL! Activar las rutas de los controladores
app.MapControllers(); 


// ENDPOINT 3: LISTADO DE ONGS (BLINDADO)
app.MapGet("/api/ongs", async (ApplicationDbContext db) =>
{
    try 
    {
        // ID DE LA PLATAFORMA
        int idPlataforma = 999; 

        // 1. Obtener la lista base
        // Nota: Usamos 'ToList' al final para traer los datos a memoria y evitar conflictos de EF
        var ongs = await db.Ongs
            .Where(o => o.EstatusId == 1 && o.Id != idPlataforma)
            .ToListAsync();
        
        // 2. Obtener catálogos (Manejando posibles nulos)
        var tipos = await db.TiposOng.ToListAsync();
        var paises = await db.Paises.ToListAsync();
        
        // 3. Intentamos obtener los montos. Si la vista falla, usamos lista vacía para que no truene todo.
        List<VistaMontoTotalPorOng> montos = new List<VistaMontoTotalPorOng>();
        try {
            montos = await db.VistasMontoOngGeneral.ToListAsync();
        } catch { 
            Console.WriteLine("Advertencia: No se pudo leer la vista VistasMontoOngGeneral."); 
        }

        // 4. Construir resultado en memoria
        var resultado = ongs.Select(o => {
            // Buscamos con seguridad (First or Default devuelve null si no halla)
            var montoData = montos.FirstOrDefault(m => m.NombreONG == o.Nombre);
            
            // Ojo aquí: Asegúrate de comparar con las propiedades correctas de tus modelos
            // Si en tu modelo TipoOng la llave es 'Tipo_ONGID', usa t.Tipo_ONGID. Si es 'Id', usa t.Id
            var tipo = tipos.FirstOrDefault(t => t.Id == o.TipoOngId); 
            var pais = paises.FirstOrDefault(p => p.Id == o.PaisId); // Verifica si es p.Id o p.PaisesID

            return new OngResumenDto
            {
                Id = o.Id,
                Nombre = o.Nombre,
                Descripcion = o.Descripcion ?? "Sin descripción", // Evitar nulos
                Logo = string.IsNullOrEmpty(o.Logo) ? "" : o.Logo, 
                
                // Usamos navegación segura (?.)
                Sector = tipo?.NombreTipoONG ?? "General",
                Pais = pais?.Nombre ?? "Internacional", // Verifica si es Nombre o NombrePaises
                
                TotalRecaudado = montoData?.TotalRecibido ?? 0
            };
        }).ToList();

        return Results.Ok(resultado);
    }
    catch (Exception ex)
    {
        // ESTO ES LO IMPORTANTE: Imprime el error real en la consola negra
        Console.WriteLine($"ERROR CRÍTICO EN API/ONGS: {ex.Message}");
        if(ex.InnerException != null) Console.WriteLine($"DETALLE: {ex.InnerException.Message}");
        
        return Results.Problem("Error interno: " + ex.Message);
    }
})
.WithName("GetOngs");

// CATÁLOGOS (Filtros)
app.MapGet("/api/catalogos", async (ApplicationDbContext db) =>
{
    // Usamos nombres de propiedades C# (Id, Tipo_ONGID, etc segun tu modelo)
    // Si falla 't.Id', prueba 't.Tipo_ONGID'
    var sectores = await db.TiposOng.Select(t => new { id = t.Id, nombre = t.NombreTipoONG }).ToListAsync();
    var paises = await db.Paises.Select(p => new { id = p.Id, nombre = p.Nombre }).ToListAsync();
    return Results.Ok(new { sectores, paises });
})
.WithName("GetCatalogos");

// PERFIL USUARIO
app.MapGet("/api/mi-cuenta/{usuarioId}", async (int usuarioId, ApplicationDbContext db) =>
{
    var datosUsuario = await db.VistaDatosUsuario
        .FromSqlInterpolated($"SELECT * FROM dbo.Vista_DatosUsuario({usuarioId})")
        .FirstOrDefaultAsync();

    if (datosUsuario == null) return Results.NotFound(new { message = "Usuario no encontrado." });
    
    var historial = await db.VistaHistorialDonaciones
        .FromSqlInterpolated($"SELECT Recibo_Economico_ID, Fecha, Monto, Nombre_ONG FROM dbo.Vista_Recibos_PorUsuario({usuarioId}) ORDER BY Fecha DESC")
        .ToListAsync();

    var responseDto = new HistorialUsuarioDto 
    {
        Nombres = datosUsuario.Nombres,
        ApellidoPaterno = datosUsuario.ApellidoPaterno,
        ApellidoMaterno = datosUsuario.ApellidoMaterno,
        CorreoElectronico = datosUsuario.CorreoElectronico,
        Pais = datosUsuario.Pais,
        Rol = datosUsuario.Rol,
        TotalDonado = datosUsuario.TotalDonado,
        DonacionesRealizadas = datosUsuario.DonacionesRealizadas,
        Historial = historial
    };

    return Results.Ok(responseDto);
});

// REGISTRO (Usuario Normal)
app.MapPost("/api/registro", async (RegistroUsuarioDto registro, ApplicationDbContext db) =>
{
    if (string.IsNullOrEmpty(registro.Correo) || string.IsNullOrEmpty(registro.Contrasena))
        return Results.BadRequest(new { message = "Datos incompletos." });

    var existe = await db.Usuarios.AnyAsync(u => u.CorreoElectronico == registro.Correo);
    if (existe) return Results.Conflict(new { message = "El correo ya existe." });

    try 
    {
        var pNombres = new SqlParameter("@Nombres", registro.Nombres);
        var pApellidoP = new SqlParameter("@ApellidoP", registro.ApellidoPaterno);
        var pApellidoM = new SqlParameter("@ApellidoM", registro.ApellidoMaterno);
        var pCorreo = new SqlParameter("@Correo", registro.Correo);
        var pContrasena = new SqlParameter("@Contrasena", registro.Contrasena);
        var pRolID = new SqlParameter("@RolID", 1); 
        var pPaisID = new SqlParameter("@PaisID", registro.PaisId);
        var pTipoCuentaID = new SqlParameter("@TipoCuentaID", registro.TipoCuentaId);

        await db.Database.ExecuteSqlRawAsync(
            "EXEC sp_Insertar_Usuario @Nombres, @ApellidoP, @ApellidoM, @Correo, @Contrasena, @RolID, @PaisID, @TipoCuentaID", 
            pNombres, pApellidoP, pApellidoM, pCorreo, pContrasena, pRolID, pPaisID, pTipoCuentaID
        );

        return Results.Ok(new { message = "Registro exitoso." });
    }
    catch (Exception ex)
    {
        return Results.Problem("Error en BD: " + ex.Message);
    }
});

// LOGIN
app.MapPost("/api/login", async (LoginDto login, ApplicationDbContext db) =>
{
    var pCorreo = new SqlParameter("@Correo", login.Correo);
    var pPassword = new SqlParameter("@PasswordTextoPlano", login.Contrasena);

    var resultado = await db.LoginResults
        .FromSqlRaw("EXEC sp_Validar_Login @Correo, @PasswordTextoPlano", pCorreo, pPassword)
        .ToListAsync();

    if (resultado.Count == 0) return Results.Unauthorized();

    var user = resultado.First();
    
    // Validación de ONG Rechazada
    if (user.TipoUsuario == "ONG")
    {
        var estatus = await db.Ongs
            .Where(o => o.Id == user.ID_Rol_o_Entidad) // Usamos o.Id (C#)
            .Select(o => o.EstatusId) // Usamos o.EstatusID (C#)
            .FirstOrDefaultAsync();
            
        // 3 = Rechazada (ajusta según tu BD)
        if (estatus == 3) 
             return Results.Json(new { message = "Tu organización fue dada de baja.", bloqueado = true }, statusCode: 403);
    }

    return Results.Ok(new { 
        message = "Login exitoso",
        usuarioId = user.ID, 
        nombre = user.NombreCompleto ?? "", 
        tipo = user.TipoUsuario, 
        rolId = user.ID_Rol_o_Entidad
    });
});

// CONTACTO
app.MapPost("/api/contacto", async (ContactoDto contacto, IEmailService emailService) =>
{
    if (contacto == null) return Results.BadRequest();
    bool success = await emailService.SendContactForm(contacto);
    return success ? Results.Ok(new { message = "Enviado" }) : Results.Json(new { message = "Error" }, statusCode: 500);
});

// PERFIL ONG (SIDEBAR)
app.MapGet("/api/ong-perfil/{id}", async (int id, ApplicationDbContext db) =>
{
    var ong = await db.Ongs
        .Where(o => o.Id == id)
        .Select(o => new { o.Id, o.Nombre, o.Logo, o.EstatusId })
        .FirstOrDefaultAsync();

    if (ong == null) return Results.NotFound();
    return Results.Ok(ong);
});

app.Run();