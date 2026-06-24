using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public interface IProductoService
{
    Task<List<Producto>> ObtenerTodosAsync();
    Task<List<Producto>> ObtenerActivosAsync();
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<Producto> CrearAsync(Producto producto);
    Task<Producto?> ActualizarAsync(int id, Producto producto);
    Task<bool> EliminarAsync(int id);
}

public class ProductoService : IProductoService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITenantService _tenantService;

    public ProductoService(IDbContextFactory<ApplicationDbContext> contextFactory, ITenantService tenantService)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
    }

    public async Task<List<Producto>> ObtenerTodosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        if (isSuperAdmin)
        {
            return await context.Productos
                .Include(p => p.Empresa)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
        else if (empresaId.HasValue)
        {
            return await context.Productos
                .Where(p => p.EmpresaId == empresaId.Value)
                .Include(p => p.Empresa)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        return new List<Producto>();
    }

    public async Task<List<Producto>> ObtenerActivosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        if (isSuperAdmin)
        {
            return await context.Productos
                .Where(p => p.Activo)
                .Include(p => p.Empresa)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }
        else if (empresaId.HasValue)
        {
            return await context.Productos
                .Where(p => p.EmpresaId == empresaId.Value && p.Activo)
                .Include(p => p.Empresa)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        return new List<Producto>();
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Productos.FindAsync(id);
    }

    public async Task<Producto> CrearAsync(Producto producto)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();

        if (!isSuperAdmin && !empresaId.HasValue)
        {
            throw new InvalidOperationException("No se puede crear un producto sin una empresa asignada.");
        }

        if (!isSuperAdmin)
        {
            producto.EmpresaId = empresaId!.Value;
        }
        
        producto.FechaCreacion = DateTime.Now;
        producto.FechaUltimaActualizacion = DateTime.Now;
        
        context.Productos.Add(producto);
        await context.SaveChangesAsync();
        
        Console.WriteLine($"ProductoService - Producto creado con ID {producto.Id}");
        return producto;
    }

    public async Task<Producto?> ActualizarAsync(int id, Producto producto)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var existente = await context.Productos.FindAsync(id);
        if (existente == null)
        {
            Console.WriteLine($"ProductoService - Producto con ID {id} no encontrado");
            return null;
        }

        // Actualizar todos los campos
        existente.Nombre = producto.Nombre;
        existente.Referencia = producto.Referencia;
        existente.CodigoBarras = producto.CodigoBarras;
        existente.Descripcion = producto.Descripcion;
        existente.Categoria = producto.Categoria;
        existente.Marca = producto.Marca;
        existente.Tipo = producto.Tipo;
        existente.UnidadMedida = producto.UnidadMedida;
        existente.PrecioCompra = producto.PrecioCompra;
        existente.PrecioUnitario = producto.PrecioUnitario;
        existente.IVA = producto.IVA;
        existente.Descuento = producto.Descuento;
        existente.Stock = producto.Stock;
        existente.StockMinimo = producto.StockMinimo;
        existente.StockMaximo = producto.StockMaximo;
        existente.Peso = producto.Peso;
        existente.Alto = producto.Alto;
        existente.Ancho = producto.Ancho;
        existente.Largo = producto.Largo;
        existente.Imagen = producto.Imagen;
        existente.Observaciones = producto.Observaciones;
        existente.Activo = producto.Activo;
        existente.FechaUltimaActualizacion = DateTime.Now;

        await context.SaveChangesAsync();
        
        Console.WriteLine($"ProductoService - Producto {id} actualizado");
        return existente;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var producto = await context.Productos.FindAsync(id);
        if (producto == null)
        {
            Console.WriteLine($"ProductoService - Producto con ID {id} no encontrado");
            return false;
        }
        
        context.Productos.Remove(producto);
        await context.SaveChangesAsync();
        
        Console.WriteLine($"ProductoService - Producto {id} eliminado");
        return true;
    }
}
