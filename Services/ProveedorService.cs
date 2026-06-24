using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public interface IProveedorService
{
    Task<List<Proveedor>> ObtenerTodosAsync();
    Task<Proveedor?> ObtenerPorIdAsync(int id);
    Task<Proveedor> CrearAsync(Proveedor proveedor);
    Task<Proveedor?> ActualizarAsync(int id, Proveedor proveedor);
    Task<bool> EliminarAsync(int id);
}

public class ProveedorService : IProveedorService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITenantService _tenantService;

    public ProveedorService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ITenantService tenantService)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
    }

    public async Task<List<Proveedor>> ObtenerTodosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        Console.WriteLine($"ProveedorService.ObtenerTodosAsync - IsSuperAdmin: {isSuperAdmin}, EmpresaId: {empresaId}");

        if (isSuperAdmin)
        {
            return await context.Proveedores
                .Include(p => p.Empresa)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
        else if (empresaId.HasValue)
        {
            return await context.Proveedores
                .Where(p => p.EmpresaId == empresaId.Value)
                .Include(p => p.Empresa)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        return new List<Proveedor>();
    }

    public async Task<Proveedor?> ObtenerPorIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        Console.WriteLine($"ProveedorService.ObtenerPorIdAsync - ID: {id}, IsSuperAdmin: {isSuperAdmin}, EmpresaId: {empresaId}");

        var proveedor = await context.Proveedores
            .Include(p => p.Empresa)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (proveedor == null)
        {
            Console.WriteLine("Proveedor no encontrado");
            return null;
        }

        // Verificar permisos
        if (!isSuperAdmin && (!empresaId.HasValue || proveedor.EmpresaId != empresaId.Value))
        {
            Console.WriteLine($"Sin permisos. Proveedor.EmpresaId: {proveedor.EmpresaId}, Session EmpresaId: {empresaId}");
            return null;
        }

        Console.WriteLine($"Proveedor encontrado: {proveedor.Nombre}");
        return proveedor;
    }

    public async Task<Proveedor> CrearAsync(Proveedor proveedor)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        Console.WriteLine($"ProveedorService.CrearAsync - IsSuperAdmin: {isSuperAdmin}, EmpresaId: {empresaId}");

        // Si no es SuperAdmin, debe tener una empresa
        if (!isSuperAdmin)
        {
            if (!empresaId.HasValue)
            {
                throw new InvalidOperationException("No se puede crear el proveedor sin una empresa asociada.");
            }
            proveedor.EmpresaId = empresaId.Value;
        }

        proveedor.FechaCreacion = DateTime.Now;
        context.Proveedores.Add(proveedor);
        await context.SaveChangesAsync();
        
        Console.WriteLine($"Proveedor creado con ID: {proveedor.Id}");
        return proveedor;
    }

    public async Task<Proveedor?> ActualizarAsync(int id, Proveedor proveedor)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        Console.WriteLine($"ProveedorService.ActualizarAsync - ID: {id}, IsSuperAdmin: {isSuperAdmin}");

        var proveedorExistente = await context.Proveedores.FindAsync(id);
        if (proveedorExistente == null)
        {
            Console.WriteLine("Proveedor no encontrado para actualizar");
            return null;
        }

        // Verificar permisos
        if (!isSuperAdmin && (!empresaId.HasValue || proveedorExistente.EmpresaId != empresaId.Value))
        {
            Console.WriteLine("Sin permisos para actualizar el proveedor");
            return null;
        }

        // Actualizar campos
        proveedorExistente.Nombre = proveedor.Nombre;
        proveedorExistente.NIF = proveedor.NIF;
        proveedorExistente.Direccion = proveedor.Direccion;
        proveedorExistente.CodigoPostal = proveedor.CodigoPostal;
        proveedorExistente.Ciudad = proveedor.Ciudad;
        proveedorExistente.Provincia = proveedor.Provincia;
        proveedorExistente.Pais = proveedor.Pais;
        proveedorExistente.Email = proveedor.Email;
        proveedorExistente.Telefono = proveedor.Telefono;
        proveedorExistente.Fax = proveedor.Fax;
        proveedorExistente.Web = proveedor.Web;
        proveedorExistente.PersonaContacto = proveedor.PersonaContacto;
        proveedorExistente.CuentaBancaria = proveedor.CuentaBancaria;
        proveedorExistente.FormaPago = proveedor.FormaPago;
        proveedorExistente.DiasPago = proveedor.DiasPago;
        proveedorExistente.Observaciones = proveedor.Observaciones;
        proveedorExistente.Activo = proveedor.Activo;

        await context.SaveChangesAsync();
        Console.WriteLine("Proveedor actualizado correctamente");
        
        return proveedorExistente;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        Console.WriteLine($"ProveedorService.EliminarAsync - ID: {id}, IsSuperAdmin: {isSuperAdmin}");

        var proveedor = await context.Proveedores.FindAsync(id);
        if (proveedor == null)
        {
            Console.WriteLine("Proveedor no encontrado para eliminar");
            return false;
        }

        // Verificar permisos
        if (!isSuperAdmin && (!empresaId.HasValue || proveedor.EmpresaId != empresaId.Value))
        {
            Console.WriteLine("Sin permisos para eliminar el proveedor");
            return false;
        }

        context.Proveedores.Remove(proveedor);
        await context.SaveChangesAsync();
        
        Console.WriteLine("Proveedor eliminado correctamente");
        return true;
    }
}
