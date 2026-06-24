using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using FactioX.Models;

namespace FactioX.Services;

public interface ITenantService
{
    Task<int?> GetEmpresaIdAsync();
    Task SetEmpresaIdAsync(int empresaId);
    Task<bool> IsSuperAdminAsync();
    Task ClearEmpresaIdAsync();
}

public class TenantService : ITenantService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private int? _empresaIdCache;
    private bool? _isSuperAdminCache;

    public TenantService(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public async Task<int?> GetEmpresaIdAsync()
    {
        if (_empresaIdCache.HasValue)
            return _empresaIdCache.Value;

        try
        {
            var result = await _sessionStorage.GetAsync<int>("EmpresaId");
            if (result.Success)
            {
                _empresaIdCache = result.Value;
                return result.Value;
            }
        }
        catch
        {
            // Si hay error al leer, devolver null
        }

        return null;
    }

    public async Task SetEmpresaIdAsync(int empresaId)
    {
        _empresaIdCache = empresaId;
        await _sessionStorage.SetAsync("EmpresaId", empresaId);
    }

    public async Task<bool> IsSuperAdminAsync()
    {
        if (_isSuperAdminCache.HasValue)
            return _isSuperAdminCache.Value;

        try
        {
            var result = await _sessionStorage.GetAsync<string>("userRole");
            if (result.Success)
            {
                _isSuperAdminCache = result.Value == Roles.SuperAdministrador;
                return _isSuperAdminCache.Value;
            }
        }
        catch
        {
            // Si hay error, devolver false
        }

        return false;
    }

    public async Task ClearEmpresaIdAsync()
    {
        _empresaIdCache = null;
        _isSuperAdminCache = null;
        await _sessionStorage.DeleteAsync("EmpresaId");
    }
}
