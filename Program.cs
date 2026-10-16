using FactioX.Components;
using FactioX.Services;
using FactioX.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Claves de Data Protection persistentes: evitan que un reciclado de IIS invalide sesiones
// y permiten descifrar los secretos guardados (contraseña del certificado).
var rutaClaves = builder.Configuration["DataProtection:KeysPath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("FactioX")
    .PersistKeysToFileSystem(new DirectoryInfo(rutaClaves));
if (OperatingSystem.IsWindows())
{
    dataProtection.ProtectKeysWithDpapi(protectToLocalMachine: true);
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar Entity Framework Core con MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Registrar servicios multi-tenant y autenticación
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ISecretosService, SecretosService>();

// Registrar servicios de la aplicación
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IPresupuestoService, PresupuestoService>();
builder.Services.AddScoped<IFacturaService, FacturaService>();
builder.Services.AddScoped<IConfiguracionEmpresaService, ConfiguracionEmpresaService>();
builder.Services.AddScoped<IFormaPagoPersonalizadaService, FormaPagoPersonalizadaService>();
builder.Services.AddScoped<IFacturaPdfService, FacturaPdfService>();
builder.Services.AddScoped<IPresupuestoPdfService, PresupuestoPdfService>();
builder.Services.AddScoped<IFacturaEService, FacturaEService>();
builder.Services.AddScoped<IFirmaDigitalService, FirmaDigitalService>();
builder.Services.AddScoped<ISEPAService, SEPAService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IDatosPruebaService, DatosPruebaService>();
builder.Services.AddScoped<IModuloService, ModuloService>();
builder.Services.AddScoped<ITpvService, TpvService>();
builder.Services.AddScoped<ITicketPdfService, TicketPdfService>();

// Registrar servicio de IA (Ollama)
builder.Services.AddHttpClient<IIAService, IAService>();
builder.Services.AddScoped<IChatContextService, ChatContextService>();

// Registrar servicio de OCR para facturas
builder.Services.AddHttpClient<IInvoiceOcrService, InvoiceOcrService>();
builder.Services.AddScoped<IManualOcrService, ManualOcrService>();

// Configurar cultura española para formateo de moneda y fechas
var cultureInfo = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();

// Cifrar las contraseñas de certificado que aún estén en texto plano
try
{
    using var scope = app.Services.CreateScope();
    var secretos = scope.ServiceProvider.GetRequiredService<ISecretosService>();
    await using var db = await scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContextAsync();
    var pendientes = await db.ConfiguracionEmpresa
        .Where(c => c.CertificadoPassword != null && c.CertificadoPassword != "" && !c.CertificadoPassword.StartsWith("dp1:"))
        .ToListAsync();
    foreach (var config in pendientes)
    {
        config.CertificadoPassword = secretos.Proteger(config.CertificadoPassword);
    }
    if (pendientes.Count > 0)
    {
        await db.SaveChangesAsync();
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "No se pudieron cifrar las contraseñas de certificado existentes.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Certificados antiguos guardados en wwwroot: nunca se sirven como estáticos
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/certificados", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
