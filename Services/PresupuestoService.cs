using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public interface IPresupuestoService
{
    Task<List<Presupuesto>> ObtenerTodosAsync();
    Task<Presupuesto?> ObtenerPorIdAsync(int id);
    Task<Presupuesto> CrearAsync(Presupuesto presupuesto);
    Task<Presupuesto?> ActualizarAsync(int id, Presupuesto presupuesto);
    Task<bool> EliminarAsync(int id);
    Task<string> GenerarNumeroAsync();
}

public class PresupuestoService : IPresupuestoService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITenantService _tenantService;

    public PresupuestoService(IDbContextFactory<ApplicationDbContext> contextFactory, ITenantService tenantService)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
    }

    public async Task<List<Presupuesto>> ObtenerTodosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var query = context.Presupuestos
            .Include(p => p.Cliente)
            .Include(p => p.Lineas)
            .AsQueryable();
        
        // SuperAdmin ve todos, otros solo ven los de su empresa
        if (!isSuperAdmin && empresaId.HasValue)
        {
            query = query.Where(p => p.EmpresaId == empresaId.Value);
        }
        
        return await query.OrderByDescending(p => p.Fecha).ToListAsync();
    }

    public async Task<Presupuesto?> ObtenerPorIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var presupuesto = await context.Presupuestos
            .Include(p => p.Cliente)
            .Include(p => p.Lineas)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        // Verificar que el presupuesto pertenece a la empresa del usuario
        if (presupuesto != null && !isSuperAdmin && presupuesto.EmpresaId != empresaId)
        {
            return null;
        }
        
        return presupuesto;
    }

    public async Task<Presupuesto> CrearAsync(Presupuesto presupuesto)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        
        // Asignar empresa del usuario actual
        if (empresaId.HasValue)
        {
            presupuesto.EmpresaId = empresaId.Value;
        }
        else
        {
            throw new InvalidOperationException("No se puede crear un presupuesto sin empresa asignada");
        }
        
        if (string.IsNullOrEmpty(presupuesto.Numero))
        {
            presupuesto.Numero = await GenerarNumeroAsync();
        }
        
        // Calcular importes de todas las líneas antes de guardar
        foreach (var linea in presupuesto.Lineas)
        {
            linea.CalcularImportes();
            // Limpiar referencias de navegación
            linea.Factura = null;
            linea.Presupuesto = null;
            linea.Producto = null;
        }
        
        // Limpiar referencias de navegación del presupuesto
        presupuesto.Cliente = null;
        presupuesto.Empresa = null!;
        
        context.Presupuestos.Add(presupuesto);
        await context.SaveChangesAsync();
        
        return presupuesto;
    }

    public async Task<Presupuesto?> ActualizarAsync(int id, Presupuesto presupuesto)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var existente = await context.Presupuestos
            .Include(p => p.Lineas)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (existente == null) return null;
        
        // Verificar que pertenece a la empresa del usuario
        if (!isSuperAdmin && existente.EmpresaId != empresaId)
        {
            return null;
        }

        existente.Fecha = presupuesto.Fecha;
        existente.FechaValidez = presupuesto.FechaValidez;
        existente.ClienteId = presupuesto.ClienteId;
        existente.Estado = presupuesto.Estado;
        existente.Notas = presupuesto.Notas;
        existente.CondicionesPago = presupuesto.CondicionesPago;
        
        // Calcular importes de todas las líneas antes de guardar
        foreach (var linea in presupuesto.Lineas)
        {
            linea.CalcularImportes();
            // Limpiar referencias de navegación
            linea.Factura = null;
            linea.Presupuesto = null;
            linea.Producto = null;
        }
        
        // Actualizar líneas
        context.LineasDocumento.RemoveRange(existente.Lineas);
        existente.Lineas = presupuesto.Lineas;

        await context.SaveChangesAsync();
        return existente;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var presupuesto = await context.Presupuestos.FindAsync(id);
        if (presupuesto == null) return false;
        
        // Verificar que pertenece a la empresa del usuario
        if (!isSuperAdmin && presupuesto.EmpresaId != empresaId)
        {
            return false;
        }
        
        context.Presupuestos.Remove(presupuesto);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerarNumeroAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        
        var year = DateTime.Now.Year;
        var query = context.Presupuestos.AsQueryable();
        
        if (empresaId.HasValue)
        {
            query = query.Where(p => p.EmpresaId == empresaId.Value);
        }
        
        var ultimoNumero = await query
            .Where(p => p.Numero.StartsWith($"PRE-{year}-"))
            .OrderByDescending(p => p.Numero)
            .Select(p => p.Numero)
            .FirstOrDefaultAsync();
        
        int siguiente = 1;
        if (!string.IsNullOrEmpty(ultimoNumero))
        {
            var partes = ultimoNumero.Split('-');
            if (partes.Length >= 3 && int.TryParse(partes[2], out int num))
            {
                siguiente = num + 1;
            }
        }
        
        return $"PRE-{year}-{siguiente:D4}";
    }
}
