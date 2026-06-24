using FactioX.Models;

namespace FactioX.Services;

public interface IChatContextService
{
    Task<string> ObtenerContextoEmpresaAsync();
}

public class ChatContextService : IChatContextService
{
    private readonly IClienteService _clienteService;
    private readonly IFacturaService _facturaService;
    private readonly IProductoService _productoService;
    private readonly ITenantService _tenantService;

    public ChatContextService(
        IClienteService clienteService,
        IFacturaService facturaService,
        IProductoService productoService,
        ITenantService tenantService)
    {
        _clienteService = clienteService;
        _facturaService = facturaService;
        _productoService = productoService;
        _tenantService = tenantService;
    }

    public async Task<string> ObtenerContextoEmpresaAsync()
    {
        try
        {
            var empresaId = await _tenantService.GetEmpresaIdAsync();
            if (!empresaId.HasValue)
            {
                return "No hay empresa seleccionada actualmente.";
            }

            // Obtener datos de la empresa
            var clientes = await _clienteService.ObtenerTodosAsync();
            var facturas = await _facturaService.ObtenerTodosAsync();
            var productos = await _productoService.ObtenerTodosAsync();

            // Calcular estadísticas
            var totalClientes = clientes.Count();
            var clientesActivos = clientes.Count(c => c.Activo);
            
            var totalFacturas = facturas.Count();
            var facturasVenta = facturas.Count(f => f.TipoFactura == TipoFactura.Venta);
            var facturasCompra = facturas.Count(f => f.TipoFactura == TipoFactura.Compra);
            
            var facturasBorrador = facturas.Count(f => f.Estado == EstadoFactura.Borrador);
            var facturasEmitidas = facturas.Count(f => f.Estado == EstadoFactura.Emitida);
            var facturasPagadas = facturas.Count(f => f.Estado == EstadoFactura.Pagada);
            var facturasVencidas = facturas.Count(f => f.Estado == EstadoFactura.Vencida);
            
            var totalProductos = productos.Count();
            var productosActivos = productos.Count(p => p.Activo);

            // Cálculos financieros
            var totalVentas = facturas
                .Where(f => f.TipoFactura == TipoFactura.Venta)
                .Sum(f => f.Total);
            
            var totalCompras = facturas
                .Where(f => f.TipoFactura == TipoFactura.Compra)
                .Sum(f => f.Total);

            var pendienteCobro = facturas
                .Where(f => f.TipoFactura == TipoFactura.Venta && (f.Estado == EstadoFactura.Emitida || f.Estado == EstadoFactura.Enviada))
                .Sum(f => f.Total);

            // Top 5 clientes
            var topClientes = facturas
                .Where(f => f.TipoFactura == TipoFactura.Venta && f.ClienteId.HasValue)
                .GroupBy(f => f.Cliente?.Nombre ?? "Desconocido")
                .Select(g => new { Cliente = g.Key, Total = g.Sum(f => f.Total) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            // Construir contexto
            var contexto = $@"
DATOS ACTUALES DE LA EMPRESA:

CLIENTES:
- Total de clientes: {totalClientes}
- Clientes activos: {clientesActivos}

FACTURAS:
- Total de facturas: {totalFacturas}
- Facturas de venta: {facturasVenta}
- Facturas de compra: {facturasCompra}
- Facturas en borrador: {facturasBorrador}
- Facturas emitidas: {facturasEmitidas}
- Facturas pagadas: {facturasPagadas}
- Facturas vencidas: {facturasVencidas}

PRODUCTOS/SERVICIOS:
- Total de productos: {totalProductos}
- Productos activos: {productosActivos}

FINANZAS:
- Total ventas: {totalVentas:C2}
- Total compras: {totalCompras:C2}
- Pendiente de cobro: {pendienteCobro:C2}
- Balance: {(totalVentas - totalCompras):C2}

TOP 5 CLIENTES:
{string.Join("\n", topClientes.Select((c, i) => $"{i + 1}. {c.Cliente}: {c.Total:C2}"))}

Usa esta información para responder preguntas sobre el estado actual del negocio.
";

            return contexto;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo contexto: {ex.Message}");
            return "No se pudo acceder a los datos de la empresa en este momento.";
        }
    }
}
