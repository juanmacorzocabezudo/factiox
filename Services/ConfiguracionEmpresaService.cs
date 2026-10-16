using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public interface IConfiguracionEmpresaService
{
    Task<ConfiguracionEmpresa?> ObtenerConfiguracionAsync();
    Task<ConfiguracionEmpresa?> ObtenerPorEmpresaIdAsync(int empresaId);
    Task<ConfiguracionEmpresa> ActualizarConfiguracionAsync(ConfiguracionEmpresa configuracion);
    Task<ConfiguracionEmpresa> CrearAsync(ConfiguracionEmpresa configuracion);
    Task ActualizarAsync(ConfiguracionEmpresa configuracion);
}

public class ConfiguracionEmpresaService : IConfiguracionEmpresaService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ITenantService _tenantService;
    private readonly ISecretosService _secretos;

    public ConfiguracionEmpresaService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ITenantService tenantService,
        ISecretosService secretos)
    {
        _dbContextFactory = dbContextFactory;
        _tenantService = tenantService;
        _secretos = secretos;
    }

    public async Task<ConfiguracionEmpresa?> ObtenerConfiguracionAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        // Si es SuperAdmin, devolver null o una configuración por defecto
        if (await _tenantService.IsSuperAdminAsync())
        {
            return null;
        }
        
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        if (!empresaId.HasValue)
        {
            return null;
        }
        
        var config = await context.ConfiguracionEmpresa
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId.Value);
        if (config != null)
            config.CertificadoPassword = _secretos.Desproteger(config.CertificadoPassword);
        return config;
    }

    public async Task<ConfiguracionEmpresa?> ObtenerPorEmpresaIdAsync(int empresaId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var config = await context.ConfiguracionEmpresa
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId);
        if (config != null)
        {
            // La contraseña del certificado solo se entrega a la propia empresa
            config.CertificadoPassword = empresaId == await _tenantService.GetEmpresaIdAsync()
                ? _secretos.Desproteger(config.CertificadoPassword)
                : null;
        }
        return config;
    }

    public async Task<ConfiguracionEmpresa> CrearAsync(ConfiguracionEmpresa configuracion)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var passwordPlano = configuracion.CertificadoPassword;
        configuracion.CertificadoPassword = _secretos.Proteger(passwordPlano);
        context.ConfiguracionEmpresa.Add(configuracion);
        try
        {
            await context.SaveChangesAsync();
        }
        finally
        {
            configuracion.CertificadoPassword = passwordPlano;
        }
        
        return configuracion;
    }

    public async Task ActualizarAsync(ConfiguracionEmpresa configuracion)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        var passwordPlano = configuracion.CertificadoPassword;
        configuracion.CertificadoPassword = _secretos.Proteger(passwordPlano);
        context.ConfiguracionEmpresa.Update(configuracion);
        // Los contadores TPV solo los modifica TpvService dentro de su transacción
        context.Entry(configuracion).Property(c => c.NumeroTicketActual).IsModified = false;
        context.Entry(configuracion).Property(c => c.NumeroDevolucionTicketActual).IsModified = false;
        try
        {
            await context.SaveChangesAsync();
        }
        finally
        {
            configuracion.CertificadoPassword = passwordPlano;
        }
    }

    public async Task<ConfiguracionEmpresa> ActualizarConfiguracionAsync(ConfiguracionEmpresa configuracion)
    {
        await ActualizarAsync(configuracion);
        return configuracion;
    }
}
