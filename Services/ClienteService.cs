using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public interface IClienteService
{
    Task<List<Cliente>> ObtenerTodosAsync();
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<Cliente> CrearAsync(Cliente cliente);
    Task<Cliente?> ActualizarAsync(int id, Cliente cliente);
    Task<bool> EliminarAsync(int id);
}

public class ClienteService : IClienteService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITenantService _tenantService;

    public ClienteService(IDbContextFactory<ApplicationDbContext> contextFactory, ITenantService tenantService)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
    }

    public async Task<List<Cliente>> ObtenerTodosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var query = context.Clientes.AsQueryable();
        
        // SuperAdmin ve todos, otros solo ven los de su empresa
        if (!isSuperAdmin && empresaId.HasValue)
        {
            query = query.Where(c => c.EmpresaId == empresaId.Value);
        }
        
        return await query.OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task<Cliente?> ObtenerPorIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        Console.WriteLine($"ObtenerPorIdAsync - ID: {id}, EmpresaId usuario: {empresaId?.ToString() ?? "NULL"}, IsSuperAdmin: {isSuperAdmin}");
        
        var cliente = await context.Clientes.FindAsync(id);
        
        Console.WriteLine($"ObtenerPorIdAsync - Cliente encontrado: {cliente?.Nombre ?? "NULL"}, Cliente.EmpresaId: {cliente?.EmpresaId.ToString() ?? "NULL"}");
        
        // Verificar que el cliente pertenece a la empresa del usuario
        if (cliente != null && !isSuperAdmin && cliente.EmpresaId != empresaId)
        {
            Console.WriteLine($"ObtenerPorIdAsync - Acceso denegado por filtro de empresa");
            return null;
        }
            
        return cliente;
    }

    public async Task<Cliente> CrearAsync(Cliente cliente)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        Console.WriteLine($"CrearAsync - EmpresaId: {empresaId?.ToString() ?? "NULL"}, IsSuperAdmin: {isSuperAdmin}");
        
        // Si no es SuperAdmin, debe tener empresa
        if (!empresaId.HasValue && !isSuperAdmin)
            throw new InvalidOperationException("No se puede crear un cliente sin empresa asociada");
        
        cliente.FechaRegistro = DateTime.Now;
        
        // Asignar la empresa si existe
        if (empresaId.HasValue)
        {
            cliente.EmpresaId = empresaId.Value;
        }
        
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        
        Console.WriteLine($"Cliente creado con ID: {cliente.Id}");
        return cliente;
    }

    public async Task<Cliente?> ActualizarAsync(int id, Cliente cliente)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var existente = await context.Clientes.FindAsync(id);
        if (existente == null) return null;
        
        // Verificar permisos
        if (!isSuperAdmin && existente.EmpresaId != empresaId)
            return null;

        existente.Nombre = cliente.Nombre;
        existente.Apellidos = cliente.Apellidos;
        existente.NombreEmpresa = cliente.NombreEmpresa;
        existente.Tipo = cliente.Tipo;
        existente.NIF = cliente.NIF;
        existente.Direccion = cliente.Direccion;
        existente.CodigoPostal = cliente.CodigoPostal;
        existente.Ciudad = cliente.Ciudad;
        existente.Provincia = cliente.Provincia;
        existente.Pais = cliente.Pais;
        existente.Email = cliente.Email;
        existente.Telefono = cliente.Telefono;
        existente.Fax = cliente.Fax;
        existente.Activo = cliente.Activo;
        existente.ClienteFacturaE = cliente.ClienteFacturaE;
        existente.TipoPersona = cliente.TipoPersona;
        existente.TipoResidencia = cliente.TipoResidencia;
        existente.PersonaContacto = cliente.PersonaContacto;
        existente.CodigoINE = cliente.CodigoINE;
        existente.CuentaBancaria = cliente.CuentaBancaria;
        existente.FormaPago = cliente.FormaPago;
        existente.LimiteCredito = cliente.LimiteCredito;
        existente.DiasPago = cliente.DiasPago;
        existente.Contacto = cliente.Contacto;
        
        // Facturación Masiva
        existente.ProductoRecurrenteId = cliente.ProductoRecurrenteId;
        existente.ConceptoRecurrente = cliente.ConceptoRecurrente;
        existente.ImporteRecurrente = cliente.ImporteRecurrente;
        existente.Periodicidad = cliente.Periodicidad;
        existente.UltimaFacturacion = cliente.UltimaFacturacion;
        existente.FacturacionMasivaActiva = cliente.FacturacionMasivaActiva;

        await context.SaveChangesAsync();
        return existente;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var cliente = await context.Clientes.FindAsync(id);
        if (cliente == null) return false;
        
        // Verificar permisos
        if (!isSuperAdmin && cliente.EmpresaId != empresaId)
            return false;
        
        context.Clientes.Remove(cliente);
        await context.SaveChangesAsync();
        return true;
    }
}
