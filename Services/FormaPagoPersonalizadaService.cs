using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public interface IFormaPagoPersonalizadaService
{
    Task<List<FormaPagoPersonalizada>> ObtenerTodosAsync();
    Task<List<FormaPagoPersonalizada>> ObtenerActivosAsync();
    Task<FormaPagoPersonalizada?> ObtenerPorIdAsync(int id);
    Task<FormaPagoPersonalizada> CrearAsync(FormaPagoPersonalizada formaPago);
    Task<FormaPagoPersonalizada?> ActualizarAsync(int id, FormaPagoPersonalizada formaPago);
    Task<bool> EliminarAsync(int id);
    Task InicializarFormasPredefinidasAsync();
}

public class FormaPagoPersonalizadaService : IFormaPagoPersonalizadaService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITenantService _tenantService;

    public FormaPagoPersonalizadaService(IDbContextFactory<ApplicationDbContext> contextFactory, ITenantService tenantService)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
    }

    public async Task<List<FormaPagoPersonalizada>> ObtenerTodosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        if (empresaId.HasValue)
        {
            return await context.FormasPagoPersonalizadas
                .Where(t => t.EmpresaId == empresaId.Value)
                .OrderBy(t => t.EsPredefinido ? 0 : 1)
                .ThenBy(t => t.Nombre)
                .ToListAsync();
        }

        return new List<FormaPagoPersonalizada>();
    }

    public async Task<List<FormaPagoPersonalizada>> ObtenerActivosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        if (empresaId.HasValue)
        {
            return await context.FormasPagoPersonalizadas
                .Where(t => t.EmpresaId == empresaId.Value && t.Activo)
                .OrderBy(t => t.EsPredefinido ? 0 : 1)
                .ThenBy(t => t.Nombre)
                .ToListAsync();
        }

        return new List<FormaPagoPersonalizada>();
    }

    public async Task<FormaPagoPersonalizada?> ObtenerPorIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        if (empresaId.HasValue)
        {
            return await context.FormasPagoPersonalizadas
                .Where(t => t.Id == id && t.EmpresaId == empresaId.Value)
                .FirstOrDefaultAsync();
        }

        return null;
    }

    public async Task<FormaPagoPersonalizada> CrearAsync(FormaPagoPersonalizada formaPago)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        if (empresaId.HasValue)
        {
            formaPago.EmpresaId = empresaId.Value;
            formaPago.FechaCreacion = DateTime.Now;
            formaPago.EsPredefinido = false; // Las formas creadas manualmente nunca son predefinidas

            context.FormasPagoPersonalizadas.Add(formaPago);
            await context.SaveChangesAsync();
            return formaPago;
        }

        throw new InvalidOperationException("No se puede crear una forma de pago sin empresa asociada");
    }

    public async Task<FormaPagoPersonalizada?> ActualizarAsync(int id, FormaPagoPersonalizada formaPago)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        if (!empresaId.HasValue)
            return null;

        var formaPagoExistente = await context.FormasPagoPersonalizadas
            .Where(t => t.Id == id && t.EmpresaId == empresaId.Value)
            .FirstOrDefaultAsync();

        if (formaPagoExistente == null)
            return null;

        formaPagoExistente.Nombre = formaPago.Nombre;
        formaPagoExistente.Descripcion = formaPago.Descripcion;
        formaPagoExistente.Activo = formaPago.Activo;

        await context.SaveChangesAsync();
        return formaPagoExistente;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        if (!empresaId.HasValue)
            return false;

        var formaPago = await context.FormasPagoPersonalizadas
            .Where(t => t.Id == id && t.EmpresaId == empresaId.Value)
            .FirstOrDefaultAsync();

        if (formaPago == null)
            return false;

        // No permitir eliminar formas predefinidas
        if (formaPago.EsPredefinido)
            return false;

        context.FormasPagoPersonalizadas.Remove(formaPago);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task InicializarFormasPredefinidasAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        if (!empresaId.HasValue)
            return;

        // Verificar si ya existen formas predefinidas
        var tienePredefinidos = await context.FormasPagoPersonalizadas
            .AnyAsync(t => t.EmpresaId == empresaId.Value && t.EsPredefinido);

        if (tienePredefinidos)
            return;

        // Crear formas predefinidas basadas en el enum FormaPago
        var formasPredefinidas = new List<FormaPagoPersonalizada>
        {
            new FormaPagoPersonalizada { Nombre = "Efectivo", Descripcion = "Pago en efectivo", EmpresaId = empresaId.Value, EsPredefinido = true, Activo = true },
            new FormaPagoPersonalizada { Nombre = "Transferencia", Descripcion = "Transferencia bancaria", EmpresaId = empresaId.Value, EsPredefinido = true, Activo = true },
            new FormaPagoPersonalizada { Nombre = "Tarjeta Crédito", Descripcion = "Pago con tarjeta de crédito", EmpresaId = empresaId.Value, EsPredefinido = true, Activo = true },
            new FormaPagoPersonalizada { Nombre = "Tarjeta Débito", Descripcion = "Pago con tarjeta de débito", EmpresaId = empresaId.Value, EsPredefinido = true, Activo = true },
            new FormaPagoPersonalizada { Nombre = "Bizum", Descripcion = "Pago mediante Bizum", EmpresaId = empresaId.Value, EsPredefinido = true, Activo = true },
            new FormaPagoPersonalizada { Nombre = "Domiciliación", Descripcion = "Domiciliación bancaria", EmpresaId = empresaId.Value, EsPredefinido = true, Activo = true }
        };

        context.FormasPagoPersonalizadas.AddRange(formasPredefinidas);
        await context.SaveChangesAsync();
    }
}
