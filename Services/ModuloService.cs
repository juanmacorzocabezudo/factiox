using FactioX.Data;
using FactioX.Models;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public interface IModuloService
{
    Task<bool> EstaActivoAsync(TipoModulo modulo);
    Task<List<EmpresaModulo>> ObtenerPorEmpresaAsync(int empresaId);
    Task<List<EmpresaModulo>> ObtenerTodosAsync();
    Task ActivarAsync(int empresaId, TipoModulo modulo);
    Task DesactivarAsync(int empresaId, TipoModulo modulo);
    void InvalidarCache();
}

public class ModuloService : IModuloService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITenantService _tenantService;
    private readonly IAuthService _authService;
    private readonly Dictionary<TipoModulo, (bool Activo, DateTime Caduca)> _cache = new();

    public ModuloService(IDbContextFactory<ApplicationDbContext> contextFactory, ITenantService tenantService, IAuthService authService)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
        _authService = authService;
    }

    public async Task<bool> EstaActivoAsync(TipoModulo modulo)
    {
        if (_cache.TryGetValue(modulo, out var cacheado) && cacheado.Caduca > DateTime.UtcNow)
            return cacheado.Activo;

        var empresaId = await _tenantService.GetEmpresaIdAsync();
        if (!empresaId.HasValue)
            return false;

        using var context = await _contextFactory.CreateDbContextAsync();
        var activo = await context.EmpresaModulos.AnyAsync(m =>
            m.EmpresaId == empresaId.Value && m.Modulo == modulo && m.Activo && m.Empresa.Activa);
        _cache[modulo] = (activo, DateTime.UtcNow.AddMinutes(1));
        return activo;
    }

    public async Task<List<EmpresaModulo>> ObtenerPorEmpresaAsync(int empresaId)
    {
        await ValidarSuperAdminAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EmpresaModulos.AsNoTracking().Where(m => m.EmpresaId == empresaId).ToListAsync();
    }

    public async Task<List<EmpresaModulo>> ObtenerTodosAsync()
    {
        await ValidarSuperAdminAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EmpresaModulos.AsNoTracking().ToListAsync();
    }

    public Task ActivarAsync(int empresaId, TipoModulo modulo) => CambiarEstadoAsync(empresaId, modulo, true);

    public Task DesactivarAsync(int empresaId, TipoModulo modulo) => CambiarEstadoAsync(empresaId, modulo, false);

    public void InvalidarCache() => _cache.Clear();

    private async Task CambiarEstadoAsync(int empresaId, TipoModulo modulo, bool activo)
    {
        await ValidarSuperAdminAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        if (!await context.Empresas.AnyAsync(e => e.Id == empresaId))
            throw new InvalidOperationException("La empresa no existe.");

        var registro = await context.EmpresaModulos.FirstOrDefaultAsync(m => m.EmpresaId == empresaId && m.Modulo == modulo);
        if (registro == null)
        {
            if (!activo) return;
            registro = new EmpresaModulo { EmpresaId = empresaId, Modulo = modulo };
            context.EmpresaModulos.Add(registro);
        }

        if (activo)
        {
            registro.FechaActivacion = DateTime.Now;
            registro.FechaDesactivacion = null;
            registro.UsuarioActivacionId = _authService.UsuarioId;
        }
        else
        {
            registro.FechaDesactivacion = DateTime.Now;
        }
        registro.Activo = activo;
        await context.SaveChangesAsync();
        InvalidarCache();
    }

    private async Task ValidarSuperAdminAsync()
    {
        if (!await _tenantService.IsSuperAdminAsync())
            throw new UnauthorizedAccessException("Solo el SuperAdministrador puede gestionar los módulos de las empresas.");
    }
}
