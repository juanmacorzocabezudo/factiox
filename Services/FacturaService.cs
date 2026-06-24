using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public interface IFacturaService
{
    Task<List<Factura>> ObtenerTodosAsync();
    Task<List<Factura>> ObtenerPorEmpresasAsync(List<int> empresaIds);
    Task<Factura?> ObtenerPorIdAsync(int id);
    Task<Factura?> ObtenerPorIdYEmpresasAsync(int id, List<int> empresaIds);
    Task<Factura> CrearAsync(Factura factura);
    Task<Factura?> ActualizarAsync(int id, Factura factura);
    Task<bool> EliminarAsync(int id);
    Task<string> GenerarNumeroAsync(TipoFactura tipoFactura);
    Task<Factura> CrearDesdePresupuestoAsync(int presupuestoId);
    Task<PagoFactura> RegistrarPagoAsync(PagoFactura pago);
    Task<bool> EliminarPagoAsync(int pagoId);
}

public class FacturaService : IFacturaService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITenantService _tenantService;
    private readonly IPresupuestoService _presupuestoService;
    private readonly IConfiguracionEmpresaService _configuracionService;

    public FacturaService(IDbContextFactory<ApplicationDbContext> contextFactory, ITenantService tenantService, IPresupuestoService presupuestoService, IConfiguracionEmpresaService configuracionService)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
        _presupuestoService = presupuestoService;
        _configuracionService = configuracionService;
    }

    public async Task<List<Factura>> ObtenerTodosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var query = context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Proveedor)
            .Include(f => f.Lineas)
            .Include(f => f.Pagos)
            .Include(f => f.FormaPagoPersonalizada)
            .AsQueryable();
        
        // SuperAdmin ve todos, otros solo ven los de su empresa
        if (!isSuperAdmin && empresaId.HasValue)
        {
            query = query.Where(f => f.EmpresaId == empresaId.Value);
        }
        
        return await query.OrderByDescending(f => f.FechaEmision).ToListAsync();
    }

    public async Task<List<Factura>> ObtenerPorEmpresasAsync(List<int> empresaIds)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        if (empresaIds == null || !empresaIds.Any())
        {
            return new List<Factura>();
        }
        
        return await context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Proveedor)
            .Include(f => f.Lineas)
            .Include(f => f.Pagos)
            .Include(f => f.FormaPagoPersonalizada)
            .Include(f => f.Empresa)
            .Where(f => empresaIds.Contains(f.EmpresaId))
            .OrderByDescending(f => f.FechaEmision)
            .ToListAsync();
    }

    public async Task<Factura?> ObtenerPorIdYEmpresasAsync(int id, List<int> empresaIds)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        if (empresaIds == null || !empresaIds.Any())
        {
            return null;
        }
        
        return await context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Proveedor)
            .Include(f => f.Lineas)
            .Include(f => f.Pagos)
            .Include(f => f.FormaPagoPersonalizada)
            .Where(f => f.Id == id && empresaIds.Contains(f.EmpresaId))
            .FirstOrDefaultAsync();
    }

    public async Task<Factura?> ObtenerPorIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var factura = await context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Proveedor)
            .Include(f => f.Lineas)
            .Include(f => f.Pagos)
            .Include(f => f.FormaPagoPersonalizada)
            .FirstOrDefaultAsync(f => f.Id == id);
        
        // Verificar que la factura pertenece a la empresa del usuario
        if (factura != null && !isSuperAdmin && empresaId.HasValue && factura.EmpresaId != empresaId)
        {
            return null;
        }
        
        return factura;
    }

    public async Task<Factura> CrearAsync(Factura factura)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        
        // Asignar empresa del usuario actual
        if (empresaId.HasValue)
        {
            factura.EmpresaId = empresaId.Value;
        }
        else
        {
            throw new InvalidOperationException("No se puede crear una factura sin empresa asignada");
        }
        
        if (string.IsNullOrEmpty(factura.NumeroFactura))
        {
            factura.NumeroFactura = await GenerarNumeroAsync(factura.TipoFactura);
        }
        
        // Calcular importes de todas las líneas antes de guardar
        foreach (var linea in factura.Lineas)
        {
            linea.CalcularImportes();
            // Limpiar referencias de navegación para evitar problemas al guardar
            linea.Factura = null;
            linea.Presupuesto = null;
            linea.Producto = null;
        }
        
        // Limpiar referencias de navegación de la factura
        factura.Cliente = null;
        factura.Proveedor = null;
        factura.Empresa = null!;
        factura.Presupuesto = null;
        
        // Recalcular totales de la factura
        factura.BaseImponible = factura.Lineas.Sum(l => l.Subtotal);
        factura.ImporteIVA = factura.Lineas.Sum(l => l.ImporteIVA);
        factura.Total = factura.BaseImponible + factura.ImporteIVA;
        
        context.Facturas.Add(factura);
        await context.SaveChangesAsync();
        
        return factura;
    }

    public async Task<Factura?> ActualizarAsync(int id, Factura factura)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var existente = await context.Facturas
            .Include(f => f.Lineas)
            .FirstOrDefaultAsync(f => f.Id == id);
            
        if (existente == null) return null;
        
        // Verificar que pertenece a la empresa del usuario
        if (!isSuperAdmin && existente.EmpresaId != empresaId)
        {
            return null;
        }

        existente.FechaEmision = factura.FechaEmision;
        existente.FechaVencimiento = factura.FechaVencimiento;
        existente.ClienteId = factura.ClienteId;
        existente.ProveedorId = factura.ProveedorId;
        existente.Estado = factura.Estado;
        existente.Observaciones = factura.Observaciones;
        existente.FormaPago = factura.FormaPago;
        existente.FormaPagoPersonalizadaId = factura.FormaPagoPersonalizadaId;
        existente.FechaPago = factura.FechaPago;
        existente.TipoFactura = factura.TipoFactura;
        
        // Actualizar documento adjunto
        existente.DocumentoAdjunto = factura.DocumentoAdjunto;
        existente.NombreDocumento = factura.NombreDocumento;
        existente.TipoDocumento = factura.TipoDocumento;
        
        // Calcular importes de todas las líneas antes de guardar
        foreach (var linea in factura.Lineas)
        {
            linea.CalcularImportes();
            // Limpiar referencias de navegación
            linea.Factura = null;
            linea.Presupuesto = null;
            linea.Producto = null;
        }
        
        // Recalcular totales
        existente.BaseImponible = factura.Lineas.Sum(l => l.Subtotal);
        existente.ImporteIVA = factura.Lineas.Sum(l => l.ImporteIVA);
        existente.Total = existente.BaseImponible + existente.ImporteIVA;
        
        // Actualizar líneas
        context.LineasDocumento.RemoveRange(existente.Lineas);
        existente.Lineas = factura.Lineas;

        await context.SaveChangesAsync();
        return existente;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        var isSuperAdmin = await _tenantService.IsSuperAdminAsync();
        
        var factura = await context.Facturas.FindAsync(id);
        if (factura == null) return false;
        
        // Verificar que pertenece a la empresa del usuario
        if (!isSuperAdmin && factura.EmpresaId != empresaId)
        {
            return false;
        }
        
        context.Facturas.Remove(factura);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerarNumeroAsync(TipoFactura tipoFactura)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        
        if (!empresaId.HasValue)
        {
            throw new InvalidOperationException("No se puede generar número de factura sin empresa asignada");
        }

        // Obtener la configuración de la empresa
        var config = await _configuracionService.ObtenerPorEmpresaIdAsync(empresaId.Value);
        if (config == null)
        {
            throw new InvalidOperationException("No se encontró la configuración de la empresa");
        }

        string serie;
        int numeroActual;
        int longitud;
        bool incluirAnio;

        // Seleccionar la configuración según el tipo de factura
        if (tipoFactura == TipoFactura.Compra)
        {
            serie = config.SerieFacturaCompra;
            numeroActual = config.NumeroFacturaCompraActual;
            longitud = config.LongitudNumeroFacturaCompra;
            incluirAnio = config.IncluirAñoEnSerieCompra;
        }
        else // TipoFactura.Venta
        {
            serie = config.SerieFactura;
            numeroActual = config.NumeroFacturaActual;
            longitud = config.LongitudNumeroFactura;
            incluirAnio = config.IncluirAñoEnSerie;
        }

        // Incrementar el número
        int siguienteNumero = numeroActual + 1;
        string numeroFormateado = siguienteNumero.ToString($"D{longitud}");

        // Actualizar el número en la configuración
        if (tipoFactura == TipoFactura.Compra)
        {
            config.NumeroFacturaCompraActual = siguienteNumero;
        }
        else
        {
            config.NumeroFacturaActual = siguienteNumero;
        }

        await _configuracionService.ActualizarAsync(config);

        // Generar el número de factura
        if (incluirAnio)
        {
            return $"{serie}-{DateTime.Now.Year}-{numeroFormateado}";
        }
        else
        {
            return $"{serie}-{numeroFormateado}";
        }
    }

    public async Task<Factura> CrearDesdePresupuestoAsync(int presupuestoId)
    {
        var presupuesto = await _presupuestoService.ObtenerPorIdAsync(presupuestoId);
        if (presupuesto == null)
            throw new ArgumentException("Presupuesto no encontrado");

        var factura = new Factura
        {
            ClienteId = presupuesto.ClienteId,
            PresupuestoId = presupuestoId,
            FechaEmision = DateTime.Now,
            Observaciones = presupuesto.Notas,
            EmpresaId = presupuesto.EmpresaId,
            TipoFactura = TipoFactura.Venta,
            Estado = EstadoFactura.Borrador,
            Lineas = presupuesto.Lineas.Select(l => new LineaDocumento
            {
                ProductoId = l.ProductoId,
                Descripcion = l.Descripcion,
                Cantidad = l.Cantidad,
                PrecioUnitario = l.PrecioUnitario,
                IVA = l.IVA,
                Descuento = l.Descuento
            }).ToList()
        };
        
        // Calcular totales
        factura.BaseImponible = factura.Lineas.Sum(l => l.Subtotal);
        factura.ImporteIVA = factura.Lineas.Sum(l => l.ImporteIVA);
        factura.Total = factura.BaseImponible + factura.ImporteIVA;

        return await CrearAsync(factura);
    }

    public async Task<PagoFactura> RegistrarPagoAsync(PagoFactura pago)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Verificar que la factura existe
        var factura = await context.Facturas
            .Include(f => f.Pagos)
            .Include(f => f.FormaPagoPersonalizada)
            .FirstOrDefaultAsync(f => f.Id == pago.FacturaId);
        
        if (factura == null)
            throw new ArgumentException("Factura no encontrada");
        
        // Establecer fecha de registro
        pago.FechaRegistro = DateTime.Now;
        
        // Añadir el pago
        context.PagosFactura.Add(pago);
        await context.SaveChangesAsync();
        
        // Actualizar estado de la factura
        var totalPagado = factura.Pagos.Sum(p => p.Importe) + pago.Importe;
        if (totalPagado >= factura.Total - 0.01m) // Tolerancia de 1 céntimo
        {
            factura.Estado = EstadoFactura.Pagada;
            factura.FechaPago = pago.FechaPago;
            await context.SaveChangesAsync();
        }
        
        return pago;
    }

    public async Task<bool> EliminarPagoAsync(int pagoId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var pago = await context.PagosFactura
            .Include(p => p.Factura)
            .ThenInclude(f => f.Pagos)
            .FirstOrDefaultAsync(p => p.Id == pagoId);
        
        if (pago == null)
            return false;
        
        var factura = pago.Factura;
        
        context.PagosFactura.Remove(pago);
        await context.SaveChangesAsync();
        
        // Actualizar estado de la factura
        var totalPagado = factura.Pagos.Where(p => p.Id != pagoId).Sum(p => p.Importe);
        if (totalPagado < factura.Total - 0.01m) // Ya no está completamente pagada
        {
            factura.Estado = EstadoFactura.Emitida;
            factura.FechaPago = null;
            await context.SaveChangesAsync();
        }
        
        return true;
    }
}
