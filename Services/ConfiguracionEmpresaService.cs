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
    Task<string> GenerarNumeroFacturaAsync();
    Task<string> GenerarNumeroPresupuestoAsync();
}

public class ConfiguracionEmpresaService : IConfiguracionEmpresaService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ITenantService _tenantService;

    public ConfiguracionEmpresaService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ITenantService tenantService)
    {
        _dbContextFactory = dbContextFactory;
        _tenantService = tenantService;
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
        
        return await context.ConfiguracionEmpresa
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId.Value);
    }

    public async Task<ConfiguracionEmpresa?> ObtenerPorEmpresaIdAsync(int empresaId)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        return await context.ConfiguracionEmpresa
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId);
    }

    public async Task<ConfiguracionEmpresa> CrearAsync(ConfiguracionEmpresa configuracion)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        context.ConfiguracionEmpresa.Add(configuracion);
        await context.SaveChangesAsync();
        
        return configuracion;
    }

    public async Task ActualizarAsync(ConfiguracionEmpresa configuracion)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        
        context.ConfiguracionEmpresa.Update(configuracion);
        await context.SaveChangesAsync();
    }

    public async Task<ConfiguracionEmpresa> ActualizarConfiguracionAsync(ConfiguracionEmpresa configuracion)
    {
        await ActualizarAsync(configuracion);
        return configuracion;
    }

    public async Task<string> GenerarNumeroFacturaAsync()
    {
        var configuracion = await ObtenerConfiguracionAsync();
        if (configuracion == null)
        {
            return "FAC-2026-0001";
        }
        
        var año = configuracion.IncluirAñoEnSerie ? DateTime.Now.Year.ToString() : "";
        var numero = configuracion.NumeroFacturaActual.ToString().PadLeft(configuracion.LongitudNumeroFactura, '0');
        var numeroFactura = configuracion.IncluirAñoEnSerie 
            ? $"{configuracion.SerieFactura}-{año}-{numero}"
            : $"{configuracion.SerieFactura}-{numero}";
        
        configuracion.NumeroFacturaActual++;
        await ActualizarAsync(configuracion);
        
        return numeroFactura;
    }

    public async Task<string> GenerarNumeroPresupuestoAsync()
    {
        var configuracion = await ObtenerConfiguracionAsync();
        if (configuracion == null)
        {
            return "PRES-2026-0001";
        }
        
        var número = configuracion.NumeroPresupuestoActual.ToString().PadLeft(configuracion.LongitudNumeroPresupuesto, '0');
        var numeroPresupuesto = $"{configuracion.SeriePresupuesto}-{DateTime.Now.Year}-{número}";
        
        configuracion.NumeroPresupuestoActual++;
        await ActualizarAsync(configuracion);
        
        return numeroPresupuesto;
    }
}
