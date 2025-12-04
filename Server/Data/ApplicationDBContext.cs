using Microsoft.EntityFrameworkCore;
using TransparencyServer.Models;

namespace TransparencyServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // --- Tablas Base ---
        public DbSet<Ong> Ongs { get; set; } = default!;
        public DbSet<Campana> Campanas { get; set; } = default!;
        public DbSet<Usuario> Usuarios { get; set; } = default!;
        public DbSet<Rol> Roles { get; set; } = default!;
        public DbSet<TipoOng> TiposOng { get; set; } = default!;
        public DbSet<Pais> Paises { get; set; } = default!;
        public DbSet<Contacto> Contactos { get; set; } = default!;
        public DbSet<TipoDeCuenta> TiposDeCuenta { get; set; } = default!;
        public DbSet<EstatusONG> EstatusONG { get; set; } = default!;
        public DbSet<MetodoPago> MetodosPago { get; set; } = default!;
        public DbSet<Moneda> Monedas { get; set; } = default!;
        
        // --- Tablas Relacionales ---
        public DbSet<ReciboDonativo> RecibosDonativoEconomico { get; set; } = default!;
        public DbSet<ReciboDonativoCampana> RecibosDonativoCampana { get; set; } = default!;
        public DbSet<UsuarioONG> UsuariosONG { get; set; } = default!;

        // --- Vistas ---
        public DbSet<VistaMetricaGlobal> VistasMetricaGeneral { get; set; } = default!; 
        public DbSet<VistaReporteSector> VistasDistribucionGeneral { get; set; } = default!; 
        public DbSet<VistaMontoTotalPorOng> VistasMontoOngGeneral { get; set; } = default!;
        public DbSet<VistaOngsParticipantes> VistaOngsParticipantes { get; set; } = default!;
        public DbSet<VistaTotalDonaciones> VistaTotalDonacionesGeneral { get; set; } = default!;

        // --- Funciones ---
        public DbSet<VistaFlujoMensual> VistaFlujoMensualGeneral { get; set; } = default!; 
        public DbSet<VistaDatosUsuario> VistaDatosUsuario { get; set; } = default!;
        public DbSet<VistaHistorialDonaciones> VistaHistorialDonaciones { get; set; } = default!;

        // --- RESULTADOS DE STORED PROCEDURES ---
        public DbSet<LoginResult> LoginResults { get; set; } = default!;
        // NUEVO: Agregado para recibir respuesta del registro de ONG
        public DbSet<SpRegistroResponse> SpRegistroResponses { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ============================================================
            // CONFIGURACIÓN DE TABLAS (CON TRIGGERS DE AUDITORÍA)
            // ============================================================
            
            // Es vital agregar 'HasTrigger' para que EF Core sepa cómo guardar sin errores
            
            modelBuilder.Entity<Ong>()
                .ToTable("ONGs", tb => tb.HasTrigger("TRG_Audit_ONGs"));

            modelBuilder.Entity<Campana>()
                .ToTable("Campañas", tb => tb.HasTrigger("TRG_Audit_Campanas"));

            modelBuilder.Entity<Usuario>()
                .ToTable("Usuarios", tb => tb.HasTrigger("TRG_Audit_Usuarios"));
                
            modelBuilder.Entity<UsuarioONG>()
                .ToTable("Usuarios_ONGs", tb => tb.HasTrigger("TRG_Audit_Usuarios_ONGs"));

            modelBuilder.Entity<ReciboDonativo>()
                .ToTable("Recibo_Donativo_Economico", tb => tb.HasTrigger("TRG_Audit_Recibo_Donativo_Economico"));
                
            modelBuilder.Entity<ReciboDonativoCampana>()
                .ToTable("Recibo_Donativo_EconomicoCampaña", tb => tb.HasTrigger("TRG_Audit_Recibo_Donativo_Campana"));


            // ============================================================
            // CONFIGURACIÓN DE TABLAS SIMPLES (SIN TRIGGERS)
            // ============================================================
            modelBuilder.Entity<Rol>().ToTable("Rol");
            modelBuilder.Entity<Pais>().ToTable("Paises");
            modelBuilder.Entity<TipoOng>().ToTable("Tipos_ONG");
            modelBuilder.Entity<Contacto>().ToTable("Contacto");
            modelBuilder.Entity<TipoDeCuenta>().ToTable("TiposDeCuenta");
            modelBuilder.Entity<EstatusONG>().ToTable("Estatus_ONG");
            modelBuilder.Entity<MetodoPago>().ToTable("MetodoPago");
            modelBuilder.Entity<Moneda>().ToTable("Moneda");


            // ============================================================
            // CONFIGURACIÓN DE VISTAS Y SPs (SIN LLAVE PRIMARIA)
            // ============================================================
            modelBuilder.Entity<VistaMetricaGlobal>().HasNoKey().ToView("Vista_Monto_Total_Donaciones_Economicas_General", schema: "dbo");
            modelBuilder.Entity<VistaReporteSector>().HasNoKey().ToView("Vista_DistribucionTipoONG_Donaciones_Economicas_General", schema: "dbo");
            modelBuilder.Entity<VistaMontoTotalPorOng>().HasNoKey().ToView("Vista_MontoTotal_Por_ONG_General", schema: "dbo");
            modelBuilder.Entity<VistaTotalDonaciones>().HasNoKey().ToView("Vista_Total_Donaciones_Economicas_Realizadas_General", schema: "dbo");
            modelBuilder.Entity<VistaOngsParticipantes>().HasNoKey().ToView("Vista_Total_ONGs_Participantes", schema: "dbo");

            // Funciones y SPs
            modelBuilder.Entity<VistaFlujoMensual>().HasNoKey();
            modelBuilder.Entity<VistaDatosUsuario>().HasNoKey();
            modelBuilder.Entity<VistaHistorialDonaciones>().HasNoKey();
            
            // Resultados de SPs
            modelBuilder.Entity<LoginResult>().HasNoKey();
            modelBuilder.Entity<SpRegistroResponse>().HasNoKey();

            base.OnModelCreating(modelBuilder);
        }
    }
}