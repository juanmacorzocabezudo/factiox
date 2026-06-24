using FactioX.Components;
using FactioX.Services;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
