using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public interface IEmpresaService
{
    Task<List<Empresa>> ObtenerTodasAsync();
    Task<Empresa?> ObtenerPorIdAsync(int id);
    Task<Empresa> CrearAsync(Empresa empresa);
    Task ActualizarAsync(int id, Empresa empresa);
    Task EliminarAsync(int id);
    Task<Empresa?> ObtenerPorSlugAsync(string slug);
}

public class EmpresaService : IEmpresaService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITenantService _tenantService;

    public EmpresaService(IDbContextFactory<ApplicationDbContext> contextFactory, ITenantService tenantService)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
    }

    private async Task ValidarSuperAdminAsync()
    {
        if (!await _tenantService.IsSuperAdminAsync())
            throw new UnauthorizedAccessException("Solo el SuperAdministrador puede gestionar empresas.");
    }

    public async Task<List<Empresa>> ObtenerTodasAsync()
    {
        if (!await _tenantService.IsSuperAdminAsync())
            return new List<Empresa>();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Empresas.ToListAsync();
    }

    public async Task<Empresa?> ObtenerPorIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Empresas.FindAsync(id);
    }

    public async Task<Empresa?> ObtenerPorSlugAsync(string slug)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Empresas.FirstOrDefaultAsync(e => e.Slug == slug);
    }

    public async Task<Empresa> CrearAsync(Empresa empresa)
    {
        await ValidarSuperAdminAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        empresa.FechaAlta = DateTime.Now;
        context.Empresas.Add(empresa);
        await context.SaveChangesAsync();
        return empresa;
    }

    public async Task ActualizarAsync(int id, Empresa empresa)
    {
        await ValidarSuperAdminAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var existente = await context.Empresas.FindAsync(id);
        if (existente != null)
        {
            context.Entry(existente).CurrentValues.SetValues(empresa);
            await context.SaveChangesAsync();
        }
    }

    public async Task EliminarAsync(int id)
    {
        await ValidarSuperAdminAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresa = await context.Empresas.FindAsync(id);
        if (empresa != null)
        {
            empresa.Activa = false;
            empresa.FechaBaja = DateTime.Now;
            await context.SaveChangesAsync();
        }
    }
}
