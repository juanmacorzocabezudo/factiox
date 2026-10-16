using System.Security.Cryptography;
using System.Text;
using FactioX.Data;
using FactioX.Models;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public class PagoEntradaTpv
{
    public int FormaPagoId { get; set; }
    public decimal Importe { get; set; }
    public string? Referencia { get; set; }
}

public class LineaDevolucionTpv
{
    public int LineaId { get; set; }
    public decimal Cantidad { get; set; }
}

public class FiltroTicketsTpv
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? TerminalId { get; set; }
    public int? UsuarioId { get; set; }
    public int? ClienteId { get; set; }
    public EstadoTicketTpv? Estado { get; set; }
    public string? Texto { get; set; }
    public int Maximo { get; set; } = 300;
}

public class InformeCajaTpv
{
    public TpvSesionCaja Sesion { get; set; } = null!;
    public string Terminal { get; set; } = string.Empty;
    public int NumeroTickets { get; set; }
    public int NumeroDevoluciones { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalDevoluciones { get; set; }
    public decimal TotalNeto => TotalVentas + TotalDevoluciones;
    public List<(string FormaPago, decimal Importe)> PorFormaPago { get; set; } = new();
    public List<(decimal Tipo, decimal Base, decimal Cuota)> PorIVA { get; set; } = new();
    public decimal Entradas { get; set; }
    public decimal Salidas { get; set; }
    public decimal CobrosEfectivo { get; set; }
    public decimal EfectivoEsperado { get; set; }
    public decimal? EfectivoContado { get; set; }
    public decimal? Descuadre => EfectivoContado.HasValue ? EfectivoContado.Value - EfectivoEsperado : null;
}

public interface ITpvService
{
    Task<bool> PuedeUsarTpvAsync();
    bool EsAdministrador { get; }

    Task<Producto?> BuscarProductoPorCodigoAsync(string codigo);
    Task<List<Producto>> BuscarProductosAsync(string? texto, string? categoria, int maximo = 60);
    Task<List<string>> ObtenerCategoriasAsync();
    Task<List<FormaPagoPersonalizada>> ObtenerFormasPagoAsync();
    Task<List<Cliente>> ObtenerClientesAsync();

    Task<List<TpvTerminal>> ObtenerTerminalesAsync(bool soloActivos = true);
    Task<TpvTerminal> GuardarTerminalAsync(TpvTerminal terminal);
    Task<ConfiguracionEmpresa> ObtenerConfiguracionAsync();
    Task GuardarConfiguracionAsync(ConfiguracionEmpresa cambios);

    Task<TpvSesionCaja?> ObtenerSesionAbiertaAsync(int terminalId);
    Task<TpvSesionCaja> AbrirCajaAsync(int terminalId, decimal fondoInicial);
    Task<TpvMovimientoCaja> RegistrarMovimientoAsync(int sesionId, TipoMovimientoCaja tipo, decimal importe, string concepto);
    Task<InformeCajaTpv> GenerarInformeZAsync(int sesionId);
    Task<InformeCajaTpv> CerrarCajaAsync(int sesionId, decimal efectivoContado, string? observaciones);
    Task<List<TpvSesionCaja>> ObtenerSesionesAsync(int? terminalId, int maximo = 30);

    Task<TicketTpv> AparcarAsync(TicketTpv ticket);
    Task<List<TicketTpv>> ObtenerAparcadosAsync(int terminalId);
    Task<TicketTpv?> ObtenerTicketAsync(int ticketId);
    Task AnularAsync(int ticketId);
    Task<TicketTpv> CobrarAsync(TicketTpv ticket, List<PagoEntradaTpv> pagos);
    Task<List<TicketTpv>> BuscarTicketsAsync(FiltroTicketsTpv filtro);
    Task<Dictionary<string, decimal>> ObtenerCantidadesDevueltasAsync(int ticketId);
    Task<TicketTpv> DevolverAsync(int ticketOriginalId, List<LineaDevolucionTpv> lineas, int terminalId, int formaPagoId, string motivo);

    Task<List<Presupuesto>> ObtenerPresupuestosCobrablesAsync();
    Task<TicketTpv> CrearTicketDesdePresupuestoAsync(int presupuestoId, int terminalId);

    Task<Factura> ConvertirTicketEnFacturaAsync(int ticketId, int clienteId);
    Task<Factura> FacturarTicketsAsync(int clienteId, List<int> ticketIds);
    Task<Dictionary<int, string>> ObtenerTicketsPorFacturaAsync(IEnumerable<int> facturaIds);

    static string ClaveLinea(LineaDocumento l) =>
        $"{l.ProductoId}|{l.Descripcion}|{Math.Abs(l.PrecioUnitario):F4}|{l.IVA:F2}|{l.Descuento:F2}";
}

public class TpvService : ITpvService
{
    private const decimal Tolerancia = 0.005m;

    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ITenantService _tenantService;
    private readonly IAuthService _authService;
    private readonly IModuloService _moduloService;
    private readonly IFacturaService _facturaService;

    public TpvService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ITenantService tenantService,
        IAuthService authService,
        IModuloService moduloService,
        IFacturaService facturaService)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
        _authService = authService;
        _moduloService = moduloService;
        _facturaService = facturaService;
    }

    public bool EsAdministrador => _authService.Rol == Roles.Administrador;

    #region Acceso

    public async Task<bool> PuedeUsarTpvAsync()
    {
        try
        {
            await ValidarAccesoAsync();
            return true;
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException)
        {
            return false;
        }
    }

    private async Task<(int EmpresaId, int UsuarioId)> ValidarAccesoAsync(bool requiereAdministrador = false)
    {
        if (!_authService.EstaAutenticado)
            await _authService.InicializarAsync();
        if (!_authService.EstaAutenticado || !_authService.UsuarioId.HasValue)
            throw new UnauthorizedAccessException("Debe iniciar sesión.");
        if (_authService.Rol is Roles.Asesoria or Roles.SuperAdministrador)
            throw new UnauthorizedAccessException("Su perfil no tiene acceso al TPV.");
        if (requiereAdministrador && !EsAdministrador)
            throw new UnauthorizedAccessException("Solo un Administrador puede realizar esta acción.");

        var empresaId = await _tenantService.GetEmpresaIdAsync()
            ?? throw new UnauthorizedAccessException("No hay empresa activa en la sesión.");
        if (!await _moduloService.EstaActivoAsync(TipoModulo.TPV))
            throw new InvalidOperationException("El módulo TPV no está activo para esta empresa.");
        return (empresaId, _authService.UsuarioId.Value);
    }

    #endregion

    #region Catálogo

    public async Task<Producto?> BuscarProductoPorCodigoAsync(string codigo)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        codigo = codigo.Trim();
        if (codigo.Length == 0) return null;

        using var context = await _contextFactory.CreateDbContextAsync();
        var productos = context.Productos.AsNoTracking().Where(p => p.EmpresaId == empresaId && p.Activo);
        return await productos.FirstOrDefaultAsync(p => p.CodigoBarras == codigo)
            ?? await productos.FirstOrDefaultAsync(p => p.Referencia == codigo);
    }

    public async Task<List<Producto>> BuscarProductosAsync(string? texto, string? categoria, int maximo = 60)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Productos.AsNoTracking().Where(p => p.EmpresaId == empresaId && p.Activo);
        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(p => p.Categoria == categoria);
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            query = query.Where(p => p.Nombre.Contains(t) || (p.Referencia != null && p.Referencia.Contains(t))
                || (p.CodigoBarras != null && p.CodigoBarras.Contains(t)));
        }
        return await query.OrderBy(p => p.Nombre).Take(Math.Clamp(maximo, 1, 200)).ToListAsync();
    }

    public async Task<List<string>> ObtenerCategoriasAsync()
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Productos.AsNoTracking()
            .Where(p => p.EmpresaId == empresaId && p.Activo && p.Categoria != null && p.Categoria != "")
            .Select(p => p.Categoria!).Distinct().OrderBy(c => c).ToListAsync();
    }

    public async Task<List<FormaPagoPersonalizada>> ObtenerFormasPagoAsync()
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.FormasPagoPersonalizadas.AsNoTracking()
            .Where(f => f.EmpresaId == empresaId && f.Activo).OrderBy(f => f.Nombre).ToListAsync();
    }

    public async Task<List<Cliente>> ObtenerClientesAsync()
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Clientes.AsNoTracking()
            .Where(c => c.EmpresaId == empresaId && c.Activo)
            .OrderBy(c => c.NombreEmpresa ?? c.Nombre).ToListAsync();
    }

    private static bool EsEfectivo(FormaPagoPersonalizada forma) =>
        forma.Nombre.Contains("efectivo", StringComparison.OrdinalIgnoreCase)
        || forma.Nombre.Contains("contado", StringComparison.OrdinalIgnoreCase);

    #endregion

    #region Terminales y configuración

    public async Task<List<TpvTerminal>> ObtenerTerminalesAsync(bool soloActivos = true)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TpvTerminales.AsNoTracking()
            .Where(t => t.EmpresaId == empresaId && (!soloActivos || t.Activo))
            .OrderBy(t => t.Codigo).ToListAsync();
    }

    public async Task<TpvTerminal> GuardarTerminalAsync(TpvTerminal terminal)
    {
        var (empresaId, _) = await ValidarAccesoAsync(requiereAdministrador: true);
        if (string.IsNullOrWhiteSpace(terminal.Nombre) || string.IsNullOrWhiteSpace(terminal.Codigo))
            throw new InvalidOperationException("Indique el nombre y el código del terminal.");
        if (terminal.ImpresoraAnchoMm is not (58 or 80))
            throw new InvalidOperationException("El ancho de impresora debe ser 58 u 80 mm.");

        using var context = await _contextFactory.CreateDbContextAsync();
        var codigo = terminal.Codigo.Trim().ToUpperInvariant();
        if (await context.TpvTerminales.AnyAsync(t => t.EmpresaId == empresaId && t.Codigo == codigo && t.Id != terminal.Id))
            throw new InvalidOperationException($"Ya existe un terminal con el código {codigo}.");
        var config = await context.ConfiguracionEmpresa.AsNoTracking().FirstOrDefaultAsync(c => c.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("No se encontró la configuración de la empresa.");
        var serieTerminal = string.IsNullOrWhiteSpace(terminal.SerieTicket) ? null : terminal.SerieTicket.Trim();
        if (serieTerminal != null)
        {
            ValidarSerie(serieTerminal, config.LongitudNumeroTicket, "tickets del terminal");
            var seriesReservadas = new[]
            {
                config.SerieTicket, config.SerieDevolucionTicket, config.SerieFactura,
                config.SerieFacturaCompra, config.SerieAbonoVenta, config.SerieAbonoCompra,
                config.SeriePresupuesto
            };
            if (seriesReservadas.Contains(serieTerminal, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("La serie propia del terminal debe ser distinta de las series generales.");
            if (await context.TpvTerminales.AnyAsync(t => t.EmpresaId == empresaId && t.Id != terminal.Id
                    && t.SerieTicket == serieTerminal))
                throw new InvalidOperationException("Ya existe otro terminal con esa serie propia.");
        }
        if (terminal.ClienteGenericoId.HasValue
            && !await context.Clientes.AnyAsync(c => c.Id == terminal.ClienteGenericoId && c.EmpresaId == empresaId))
            throw new InvalidOperationException("El cliente genérico no pertenece a la empresa.");
        if (terminal.FormaPagoPorDefectoId.HasValue
            && !await context.FormasPagoPersonalizadas.AnyAsync(f => f.Id == terminal.FormaPagoPorDefectoId && f.EmpresaId == empresaId))
            throw new InvalidOperationException("La forma de pago no pertenece a la empresa.");

        TpvTerminal destino;
        if (terminal.Id == 0)
        {
            destino = new TpvTerminal { EmpresaId = empresaId };
            context.TpvTerminales.Add(destino);
        }
        else
        {
            destino = await context.TpvTerminales.FirstOrDefaultAsync(t => t.Id == terminal.Id && t.EmpresaId == empresaId)
                ?? throw new InvalidOperationException("Terminal no encontrado.");
            if (!string.Equals(destino.SerieTicket, serieTerminal, StringComparison.OrdinalIgnoreCase)
                && await context.TicketsTpv.AnyAsync(t => t.TerminalId == destino.Id && t.EmpresaId == empresaId && t.Numero != null))
                throw new InvalidOperationException("No se puede cambiar la serie propia después de emitir tickets en este terminal.");
            if (!terminal.Activo && destino.Activo
                && await context.TpvSesionesCaja.AnyAsync(s => s.TerminalId == destino.Id && s.Estado == EstadoSesionCaja.Abierta))
                throw new InvalidOperationException("Cierre la caja del terminal antes de desactivarlo.");
        }

        destino.Nombre = terminal.Nombre.Trim();
        destino.Codigo = codigo;
        destino.Activo = terminal.Activo;
        destino.SerieTicket = serieTerminal;
        destino.ClienteGenericoId = terminal.ClienteGenericoId;
        destino.FormaPagoPorDefectoId = terminal.FormaPagoPorDefectoId;
        destino.ImpresoraAnchoMm = terminal.ImpresoraAnchoMm;
        await context.SaveChangesAsync();
        return destino;
    }

    public async Task<ConfiguracionEmpresa> ObtenerConfiguracionAsync()
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.ConfiguracionEmpresa.AsNoTracking().FirstOrDefaultAsync(c => c.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("No se encontró la configuración de la empresa.");
    }

    public async Task GuardarConfiguracionAsync(ConfiguracionEmpresa cambios)
    {
        var (empresaId, _) = await ValidarAccesoAsync(requiereAdministrador: true);
        ValidarSerie(cambios.SerieTicket, cambios.LongitudNumeroTicket, "tickets");
        ValidarSerie(cambios.SerieDevolucionTicket, cambios.LongitudNumeroDevolucionTicket, "devoluciones");
        if (string.Equals(cambios.SerieTicket.Trim(), cambios.SerieDevolucionTicket.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("La serie de tickets y la de devoluciones deben ser distintas.");
        if (cambios.TpvTextoPieTicket?.Length > 500)
            throw new InvalidOperationException("El texto de pie admite hasta 500 caracteres.");

        using var context = await _contextFactory.CreateDbContextAsync();
        var config = await context.ConfiguracionEmpresa.FirstOrDefaultAsync(c => c.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("No se encontró la configuración de la empresa.");

        var otrasSeries = new[] { config.SerieFactura, config.SerieFacturaCompra, config.SerieAbonoVenta, config.SerieAbonoCompra, config.SeriePresupuesto };
        if (otrasSeries.Contains(cambios.SerieTicket.Trim(), StringComparer.OrdinalIgnoreCase)
            || otrasSeries.Contains(cambios.SerieDevolucionTicket.Trim(), StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Las series del TPV deben ser distintas de las de facturas, abonos y presupuestos.");
        if (await context.TpvTerminales.AnyAsync(t => t.EmpresaId == empresaId && t.SerieTicket != null
                && (t.SerieTicket == cambios.SerieTicket.Trim() || t.SerieTicket == cambios.SerieDevolucionTicket.Trim())))
            throw new InvalidOperationException("Las series generales no pueden coincidir con las series propias de los terminales.");

        var hayTickets = await context.TicketsTpv.AnyAsync(t => t.EmpresaId == empresaId && t.Numero != null);
        if (hayTickets && (!string.Equals(config.SerieTicket, cambios.SerieTicket.Trim(), StringComparison.Ordinal)
            || !string.Equals(config.SerieDevolucionTicket, cambios.SerieDevolucionTicket.Trim(), StringComparison.Ordinal)
            || config.IncluirAnioEnSerieTicket != cambios.IncluirAnioEnSerieTicket
            || config.IncluirAnioEnSerieDevolucionTicket != cambios.IncluirAnioEnSerieDevolucionTicket))
        {
            // Cambiar la serie con tickets emitidos rompería la correlatividad de la serie en curso
            throw new InvalidOperationException("No se puede cambiar la serie ni el formato de año cuando ya hay tickets emitidos.");
        }

        config.SerieTicket = cambios.SerieTicket.Trim();
        config.LongitudNumeroTicket = cambios.LongitudNumeroTicket;
        config.IncluirAnioEnSerieTicket = cambios.IncluirAnioEnSerieTicket;
        config.SerieDevolucionTicket = cambios.SerieDevolucionTicket.Trim();
        config.LongitudNumeroDevolucionTicket = cambios.LongitudNumeroDevolucionTicket;
        config.IncluirAnioEnSerieDevolucionTicket = cambios.IncluirAnioEnSerieDevolucionTicket;
        config.TpvDescontarStock = cambios.TpvDescontarStock;
        config.TpvPermitirStockNegativo = cambios.TpvPermitirStockNegativo;
        config.TpvPreciosIncluyenIVA = cambios.TpvPreciosIncluyenIVA;
        config.TpvTextoPieTicket = string.IsNullOrWhiteSpace(cambios.TpvTextoPieTicket) ? null : cambios.TpvTextoPieTicket.Trim();
        await context.SaveChangesAsync();
    }

    private static void ValidarSerie(string? serie, int longitud, string nombre)
    {
        if (string.IsNullOrWhiteSpace(serie) || serie.Trim().Length > 10)
            throw new InvalidOperationException($"La serie de {nombre} es obligatoria y admite hasta 10 caracteres.");
        if (longitud < 3 || longitud > 8)
            throw new InvalidOperationException($"La longitud del número de {nombre} debe estar entre 3 y 8.");
    }

    #endregion

    #region Caja

    public async Task<TpvSesionCaja?> ObtenerSesionAbiertaAsync(int terminalId)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TpvSesionesCaja.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TerminalId == terminalId && s.EmpresaId == empresaId && s.Estado == EstadoSesionCaja.Abierta);
    }

    public async Task<TpvSesionCaja> AbrirCajaAsync(int terminalId, decimal fondoInicial)
    {
        var (empresaId, usuarioId) = await ValidarAccesoAsync();
        if (fondoInicial < 0)
            throw new InvalidOperationException("El fondo inicial no puede ser negativo.");

        using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaccion = await context.Database.BeginTransactionAsync();
        var terminales = await context.TpvTerminales
            .FromSqlInterpolated($"SELECT * FROM TpvTerminales WHERE Id = {terminalId} AND EmpresaId = {empresaId} FOR UPDATE")
            .ToListAsync();
        var terminal = terminales.SingleOrDefault();
        if (terminal == null || !terminal.Activo)
            throw new InvalidOperationException("Terminal no disponible.");
        if (await context.TpvSesionesCaja.AnyAsync(s => s.TerminalId == terminalId && s.Estado == EstadoSesionCaja.Abierta))
            throw new InvalidOperationException("La caja de este terminal ya está abierta.");

        var sesion = new TpvSesionCaja
        {
            EmpresaId = empresaId,
            TerminalId = terminalId,
            UsuarioAperturaId = usuarioId,
            FechaApertura = DateTime.Now,
            FondoInicial = Math.Round(fondoInicial, 2),
            Estado = EstadoSesionCaja.Abierta
        };
        context.TpvSesionesCaja.Add(sesion);
        await context.SaveChangesAsync();
        await transaccion.CommitAsync();
        return sesion;
    }

    public async Task<TpvMovimientoCaja> RegistrarMovimientoAsync(int sesionId, TipoMovimientoCaja tipo, decimal importe, string concepto)
    {
        var (empresaId, usuarioId) = await ValidarAccesoAsync();
        if (importe <= 0)
            throw new InvalidOperationException("El importe debe ser mayor que cero.");
        if (string.IsNullOrWhiteSpace(concepto) || concepto.Length > 200)
            throw new InvalidOperationException("Indique un concepto (máximo 200 caracteres).");

        using var context = await _contextFactory.CreateDbContextAsync();
        var sesion = await context.TpvSesionesCaja.FirstOrDefaultAsync(s => s.Id == sesionId && s.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("Sesión de caja no encontrada.");
        if (sesion.Estado != EstadoSesionCaja.Abierta)
            throw new InvalidOperationException("La caja está cerrada.");

        var movimiento = new TpvMovimientoCaja
        {
            SesionCajaId = sesionId,
            Tipo = tipo,
            Importe = Math.Round(importe, 2),
            Concepto = concepto.Trim(),
            UsuarioId = usuarioId,
            Fecha = DateTime.Now
        };
        context.TpvMovimientosCaja.Add(movimiento);
        await context.SaveChangesAsync();
        return movimiento;
    }

    public async Task<InformeCajaTpv> GenerarInformeZAsync(int sesionId)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await CalcularInformeAsync(context, empresaId, sesionId);
    }

    private static async Task<InformeCajaTpv> CalcularInformeAsync(ApplicationDbContext context, int empresaId, int sesionId)
    {
        var sesion = await context.TpvSesionesCaja.AsNoTracking()
            .Include(s => s.Terminal)
            .Include(s => s.Movimientos)
            .FirstOrDefaultAsync(s => s.Id == sesionId && s.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("Sesión de caja no encontrada.");

        var tickets = await context.TicketsTpv.AsNoTracking()
            .Include(t => t.Lineas)
            .Include(t => t.Pagos).ThenInclude(p => p.FormaPagoPersonalizada)
            .AsSplitQuery()
            .Where(t => t.SesionCajaId == sesionId && t.EmpresaId == empresaId
                && (t.Estado == EstadoTicketTpv.Cobrado || t.Estado == EstadoTicketTpv.Facturado))
            .ToListAsync();

        var ventas = tickets.Where(t => !t.EsDevolucion).ToList();
        var devoluciones = tickets.Where(t => t.EsDevolucion).ToList();
        var pagos = tickets.SelectMany(t => t.Pagos).ToList();
        var cobrosEfectivo = pagos.Where(p => p.EsEfectivo).Sum(p => p.Importe);
        var entradas = sesion.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Entrada).Sum(m => m.Importe);
        var salidas = sesion.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Salida).Sum(m => m.Importe);

        return new InformeCajaTpv
        {
            Sesion = sesion,
            Terminal = $"{sesion.Terminal.Codigo} - {sesion.Terminal.Nombre}",
            NumeroTickets = ventas.Count,
            NumeroDevoluciones = devoluciones.Count,
            TotalVentas = ventas.Sum(t => t.Total),
            TotalDevoluciones = devoluciones.Sum(t => t.Total),
            PorFormaPago = pagos
                .GroupBy(p => p.FormaPagoPersonalizada?.Nombre ?? (p.EsEfectivo ? "Efectivo" : "Otros"))
                .Select(g => (g.Key, g.Sum(p => p.Importe)))
                .OrderBy(x => x.Key).ToList(),
            PorIVA = tickets.SelectMany(t => t.DesgloseIVA())
                .GroupBy(d => d.Tipo)
                .Select(g => (g.Key, g.Sum(d => d.Base), g.Sum(d => d.Cuota)))
                .OrderBy(x => x.Key).ToList(),
            Entradas = entradas,
            Salidas = salidas,
            CobrosEfectivo = cobrosEfectivo,
            EfectivoEsperado = sesion.FondoInicial + cobrosEfectivo + entradas - salidas,
            EfectivoContado = sesion.EfectivoContado
        };
    }

    public async Task<InformeCajaTpv> CerrarCajaAsync(int sesionId, decimal efectivoContado, string? observaciones)
    {
        var (empresaId, usuarioId) = await ValidarAccesoAsync();
        if (efectivoContado < 0)
            throw new InvalidOperationException("El efectivo contado no puede ser negativo.");

        using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaccion = await context.Database.BeginTransactionAsync();
        var sesiones = await context.TpvSesionesCaja
            .FromSqlInterpolated($"SELECT * FROM TpvSesionesCaja WHERE Id = {sesionId} AND EmpresaId = {empresaId} FOR UPDATE")
            .ToListAsync();
        var sesion = sesiones.SingleOrDefault() ?? throw new InvalidOperationException("Sesión de caja no encontrada.");
        if (sesion.Estado != EstadoSesionCaja.Abierta)
            throw new InvalidOperationException("La caja ya está cerrada.");

        var informe = await CalcularInformeAsync(context, empresaId, sesionId);
        sesion.EfectivoEsperado = informe.EfectivoEsperado;
        sesion.EfectivoContado = Math.Round(efectivoContado, 2);
        sesion.Descuadre = sesion.EfectivoContado.Value - sesion.EfectivoEsperado;
        sesion.FechaCierre = DateTime.Now;
        sesion.UsuarioCierreId = usuarioId;
        sesion.Estado = EstadoSesionCaja.Cerrada;
        sesion.Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim()[..Math.Min(observaciones.Trim().Length, 1000)];
        await context.SaveChangesAsync();
        await transaccion.CommitAsync();

        informe.Sesion = sesion;
        informe.EfectivoContado = sesion.EfectivoContado;
        return informe;
    }

    public async Task<List<TpvSesionCaja>> ObtenerSesionesAsync(int? terminalId, int maximo = 30)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TpvSesionesCaja.AsNoTracking()
            .Include(s => s.Terminal)
            .Where(s => s.EmpresaId == empresaId && (!terminalId.HasValue || s.TerminalId == terminalId))
            .OrderByDescending(s => s.FechaApertura)
            .Take(Math.Clamp(maximo, 1, 200))
            .ToListAsync();
    }

    #endregion

    #region Tickets

    public async Task<TicketTpv> AparcarAsync(TicketTpv ticket)
    {
        var (empresaId, usuarioId) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaccion = await context.Database.BeginTransactionAsync();
        var destino = await PrepararTicketVentaAsync(context, empresaId, usuarioId, ticket);
        destino.Estado = EstadoTicketTpv.Aparcado;
        await context.SaveChangesAsync();
        await transaccion.CommitAsync();
        return destino;
    }

    public async Task<List<TicketTpv>> ObtenerAparcadosAsync(int terminalId)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TicketsTpv.AsNoTracking()
            .Include(t => t.Cliente)
            .Include(t => t.Usuario)
            .Where(t => t.EmpresaId == empresaId && t.TerminalId == terminalId && t.Estado == EstadoTicketTpv.Aparcado)
            .OrderBy(t => t.Fecha)
            .ToListAsync();
    }

    public async Task<TicketTpv?> ObtenerTicketAsync(int ticketId)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TicketsTpv.AsNoTracking()
            .Include(t => t.Lineas).ThenInclude(l => l.Producto)
            .Include(t => t.Pagos).ThenInclude(p => p.FormaPagoPersonalizada)
            .Include(t => t.Cliente)
            .Include(t => t.Terminal)
            .Include(t => t.Usuario)
            .Include(t => t.Factura)
            .Include(t => t.TicketOriginal)
            .Include(t => t.PresupuestoOrigen)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.EmpresaId == empresaId);
    }

    public async Task AnularAsync(int ticketId)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var ticket = await context.TicketsTpv.FirstOrDefaultAsync(t => t.Id == ticketId && t.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("Ticket no encontrado.");
        if (!ticket.Editable)
            throw new InvalidOperationException("Un ticket cobrado no se puede anular. Utilice una devolución.");
        ticket.Estado = EstadoTicketTpv.Anulado;
        await context.SaveChangesAsync();
    }

    public async Task<TicketTpv> CobrarAsync(TicketTpv ticket, List<PagoEntradaTpv> pagos)
    {
        var (empresaId, usuarioId) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaccion = await context.Database.BeginTransactionAsync();

        var sesion = await context.TpvSesionesCaja.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TerminalId == ticket.TerminalId && s.EmpresaId == empresaId && s.Estado == EstadoSesionCaja.Abierta)
            ?? throw new InvalidOperationException("Abra la caja del terminal antes de cobrar.");

        var destino = await PrepararTicketVentaAsync(context, empresaId, usuarioId, ticket);
        if (destino.Total <= 0)
            throw new InvalidOperationException("El total del ticket debe ser mayor que cero.");
        if (destino.PresupuestoOrigenId.HasValue)
            await ValidarPresupuestoLibreAsync(context, empresaId, destino.PresupuestoOrigenId.Value, destino.Id);

        await AsignarPagosAsync(context, empresaId, destino, pagos);

        // El bloqueo de la configuración serializa numeración, huella y stock entre terminales
        var config = await BloquearConfiguracionAsync(context, empresaId);
        destino.Numero = await SiguienteNumeroAsync(context, config, empresaId, devolucion: false, destino.TerminalId);
        destino.SesionCajaId = sesion.Id;
        destino.UsuarioId = usuarioId;
        destino.FechaCobro = DateTime.Now;
        destino.Estado = EstadoTicketTpv.Cobrado;
        destino.Huella = await CalcularHuellaAsync(context, empresaId, config, destino);
        await AjustarStockAsync(context, empresaId, config, destino.Lineas, signo: -1);

        await context.SaveChangesAsync();
        await transaccion.CommitAsync();
        return (await ObtenerTicketAsync(destino.Id))!;
    }

    private async Task<TicketTpv> PrepararTicketVentaAsync(ApplicationDbContext context, int empresaId, int usuarioId, TicketTpv origen)
    {
        if (origen.Lineas.Count == 0)
            throw new InvalidOperationException("El ticket no tiene líneas.");
        if (origen.DescuentoGlobal < 0 || origen.DescuentoGlobal > 100)
            throw new InvalidOperationException("El descuento global debe estar entre 0 y 100.");

        var terminal = await context.TpvTerminales.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == origen.TerminalId && t.EmpresaId == empresaId && t.Activo)
            ?? throw new InvalidOperationException("Terminal no disponible.");
        if (origen.ClienteId.HasValue && !await context.Clientes.AnyAsync(c => c.Id == origen.ClienteId && c.EmpresaId == empresaId))
            throw new InvalidOperationException("El cliente no pertenece a la empresa.");
        if (origen.PresupuestoOrigenId.HasValue
            && !await context.Presupuestos.AnyAsync(p => p.Id == origen.PresupuestoOrigenId && p.EmpresaId == empresaId))
            throw new InvalidOperationException("El presupuesto no pertenece a la empresa.");

        var productoIds = origen.Lineas.Where(l => l.ProductoId.HasValue).Select(l => l.ProductoId!.Value).Distinct().ToList();
        var productos = await context.Productos.AsNoTracking()
            .Where(p => productoIds.Contains(p.Id) && p.EmpresaId == empresaId)
            .ToDictionaryAsync(p => p.Id);

        var lineas = new List<LineaDocumento>();
        foreach (var linea in origen.Lineas)
        {
            if (linea.Cantidad <= 0)
                throw new InvalidOperationException("Las cantidades deben ser mayores que cero.");
            if (linea.Descuento < 0 || linea.Descuento > 100)
                throw new InvalidOperationException("El descuento de línea debe estar entre 0 y 100.");

            var nueva = new LineaDocumento
            {
                ProductoId = linea.ProductoId,
                Cantidad = Math.Round(linea.Cantidad, 2),
                Descuento = Math.Round(linea.Descuento, 2)
            };

            if (linea.ProductoId.HasValue)
            {
                if (!productos.TryGetValue(linea.ProductoId.Value, out var producto))
                    throw new InvalidOperationException("Hay productos que no pertenecen a la empresa.");
                nueva.Descripcion = string.IsNullOrWhiteSpace(linea.Descripcion) ? producto.Nombre : linea.Descripcion.Trim();
                nueva.IVA = producto.IVA;
                nueva.PrecioUnitario = EsAdministrador || origen.PresupuestoOrigenId.HasValue
                    ? Math.Round(linea.PrecioUnitario, 2)
                    : producto.PrecioUnitario;
            }
            else
            {
                if (!EsAdministrador && !origen.PresupuestoOrigenId.HasValue)
                    throw new UnauthorizedAccessException("Solo un Administrador puede añadir líneas libres.");
                if (string.IsNullOrWhiteSpace(linea.Descripcion))
                    throw new InvalidOperationException("Las líneas libres necesitan descripción.");
                if (linea.IVA < 0 || linea.IVA > 100)
                    throw new InvalidOperationException("Tipo de IVA no válido.");
                nueva.Descripcion = linea.Descripcion.Trim();
                nueva.IVA = linea.IVA;
                nueva.PrecioUnitario = Math.Round(linea.PrecioUnitario, 2);
            }
            if (nueva.PrecioUnitario < 0)
                throw new InvalidOperationException("Los precios no pueden ser negativos.");
            if (nueva.Descripcion.Length > 500)
                nueva.Descripcion = nueva.Descripcion[..500];
            lineas.Add(nueva);
        }

        TicketTpv destino;
        if (origen.Id == 0)
        {
            destino = new TicketTpv { EmpresaId = empresaId, Fecha = DateTime.Now, TipoTicket = TipoTicketTpv.Venta };
            context.TicketsTpv.Add(destino);
        }
        else
        {
            var existentes = await context.TicketsTpv
                .FromSqlInterpolated($"SELECT * FROM TicketsTpv WHERE Id = {origen.Id} AND EmpresaId = {empresaId} FOR UPDATE")
                .ToListAsync();
            destino = existentes.SingleOrDefault() ?? throw new InvalidOperationException("Ticket no encontrado.");
            if (!destino.Editable || destino.EsDevolucion)
                throw new InvalidOperationException("El ticket ya no se puede modificar.");
            var lineasAnteriores = await context.LineasDocumento.Where(l => l.TicketTpvId == destino.Id).ToListAsync();
            context.LineasDocumento.RemoveRange(lineasAnteriores);
        }

        destino.TerminalId = terminal.Id;
        destino.UsuarioId = usuarioId;
        destino.ClienteId = origen.ClienteId ?? terminal.ClienteGenericoId;
        destino.PresupuestoOrigenId = origen.PresupuestoOrigenId;
        destino.DescuentoGlobal = Math.Round(origen.DescuentoGlobal, 2);
        destino.Notas = string.IsNullOrWhiteSpace(origen.Notas) ? null : origen.Notas.Trim()[..Math.Min(origen.Notas.Trim().Length, 500)];
        destino.Lineas = lineas;
        destino.CalcularTotales();
        return destino;
    }

    private static async Task ValidarPresupuestoLibreAsync(ApplicationDbContext context, int empresaId, int presupuestoId, int ticketId)
    {
        if (await context.TicketsTpv.AnyAsync(t => t.EmpresaId == empresaId && t.PresupuestoOrigenId == presupuestoId && t.Id != ticketId
                && (t.Estado == EstadoTicketTpv.Cobrado || t.Estado == EstadoTicketTpv.Facturado)))
            throw new InvalidOperationException("Este presupuesto ya se cobró en el TPV.");
    }

    private static async Task AsignarPagosAsync(ApplicationDbContext context, int empresaId, TicketTpv ticket, List<PagoEntradaTpv> pagos)
    {
        if (pagos.Count == 0)
            throw new InvalidOperationException("Indique al menos una forma de pago.");
        if (pagos.Any(p => p.Importe <= 0))
            throw new InvalidOperationException("Los importes de pago deben ser mayores que cero.");

        var formaIds = pagos.Select(p => p.FormaPagoId).Distinct().ToList();
        var formas = await context.FormasPagoPersonalizadas.AsNoTracking()
            .Where(f => formaIds.Contains(f.Id) && f.EmpresaId == empresaId && f.Activo)
            .ToDictionaryAsync(f => f.Id);
        if (formas.Count != formaIds.Count)
            throw new InvalidOperationException("Hay formas de pago no válidas.");

        var entregado = Math.Round(pagos.Sum(p => p.Importe), 2);
        var efectivoEntregado = pagos.Where(p => EsEfectivo(formas[p.FormaPagoId])).Sum(p => p.Importe);
        var cambio = entregado - ticket.Total;
        if (cambio < -Tolerancia)
            throw new InvalidOperationException($"Faltan {(-cambio):C2} por cobrar.");
        cambio = Math.Max(0, cambio);
        if (cambio > efectivoEntregado + Tolerancia)
            throw new InvalidOperationException("Solo se puede devolver cambio de los pagos en efectivo.");

        var cambioPendiente = cambio;
        ticket.Pagos = new List<PagoTicketTpv>();
        foreach (var pago in pagos.OrderBy(p => EsEfectivo(formas[p.FormaPagoId]) ? 1 : 0))
        {
            var esEfectivo = EsEfectivo(formas[pago.FormaPagoId]);
            var importe = Math.Round(pago.Importe, 2);
            if (esEfectivo && cambioPendiente > 0)
            {
                var descuento = Math.Min(importe, cambioPendiente);
                importe -= descuento;
                cambioPendiente -= descuento;
            }
            if (importe <= 0) continue;
            ticket.Pagos.Add(new PagoTicketTpv
            {
                FormaPagoPersonalizadaId = pago.FormaPagoId,
                EsEfectivo = esEfectivo,
                Importe = importe,
                Referencia = string.IsNullOrWhiteSpace(pago.Referencia) ? null : pago.Referencia.Trim()[..Math.Min(pago.Referencia.Trim().Length, 100)]
            });
        }
        ticket.Entregado = entregado;
        ticket.Cambio = Math.Round(cambio, 2);
    }

    private static async Task<ConfiguracionEmpresa> BloquearConfiguracionAsync(ApplicationDbContext context, int empresaId)
    {
        var configuraciones = await context.ConfiguracionEmpresa
            .FromSqlInterpolated($"SELECT * FROM ConfiguracionEmpresa WHERE EmpresaId = {empresaId} FOR UPDATE")
            .ToListAsync();
        return configuraciones.SingleOrDefault()
            ?? throw new InvalidOperationException("No se encontró la configuración de la empresa.");
    }

    private static async Task<string> SiguienteNumeroAsync(
        ApplicationDbContext context,
        ConfiguracionEmpresa config,
        int empresaId,
        bool devolucion,
        int terminalId)
    {
        TpvTerminal? terminal = null;
        if (!devolucion)
        {
            terminal = await context.TpvTerminales.FirstOrDefaultAsync(t => t.Id == terminalId && t.EmpresaId == empresaId)
                ?? throw new InvalidOperationException("Terminal no encontrado.");
        }

        var serie = devolucion ? config.SerieDevolucionTicket : terminal?.SerieTicket ?? config.SerieTicket;
        var longitud = devolucion ? config.LongitudNumeroDevolucionTicket : config.LongitudNumeroTicket;
        var incluirAnio = devolucion ? config.IncluirAnioEnSerieDevolucionTicket : config.IncluirAnioEnSerieTicket;
        var seriePropia = !devolucion && terminal?.SerieTicket != null;
        var actual = devolucion ? config.NumeroDevolucionTicketActual
            : seriePropia ? terminal!.NumeroTicketActual : config.NumeroTicketActual;
        ValidarSerie(serie, longitud, devolucion ? "devoluciones" : "tickets");
        if (actual < 0)
            throw new InvalidOperationException("Revise el contador de tickets en la configuración.");

        var siguiente = checked(actual + 1);
        if (devolucion) config.NumeroDevolucionTicketActual = siguiente;
        else if (seriePropia) terminal!.NumeroTicketActual = siguiente;
        else config.NumeroTicketActual = siguiente;

        var numero = siguiente.ToString($"D{longitud}");
        var resultado = incluirAnio ? $"{serie}-{DateTime.Today.Year}-{numero}" : $"{serie}-{numero}";
        if (await context.TicketsTpv.AnyAsync(t => t.EmpresaId == empresaId && t.Numero == resultado))
            throw new InvalidOperationException($"El número {resultado} ya existe. Revise el contador de la serie.");
        return resultado;
    }

    private static async Task<string> CalcularHuellaAsync(ApplicationDbContext context, int empresaId, ConfiguracionEmpresa config, TicketTpv ticket)
    {
        var anterior = await context.TicketsTpv.AsNoTracking()
            .Where(t => t.EmpresaId == empresaId && t.Huella != null)
            .OrderByDescending(t => t.FechaCobro).ThenByDescending(t => t.Id)
            .Select(t => t.Huella)
            .FirstOrDefaultAsync();
        var datos = string.Join('|', config.NIF, ticket.Numero, ticket.FechaCobro!.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
            ticket.Total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture), anterior ?? string.Empty);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(datos)));
    }

    private static async Task AjustarStockAsync(ApplicationDbContext context, int empresaId, ConfiguracionEmpresa config,
        IEnumerable<LineaDocumento> lineas, int signo)
    {
        if (!config.TpvDescontarStock) return;

        var cantidades = lineas.Where(l => l.ProductoId.HasValue)
            .GroupBy(l => l.ProductoId!.Value)
            .Select(g => (ProductoId: g.Key, Unidades: (int)Math.Round(Math.Abs(g.Sum(l => l.Cantidad)), MidpointRounding.AwayFromZero)))
            .Where(x => x.Unidades > 0)
            .ToList();

        foreach (var (productoId, unidades) in cantidades)
        {
            var delta = signo * unidades;
            var query = context.Productos.Where(p => p.Id == productoId && p.EmpresaId == empresaId
                && p.Tipo == TipoProducto.Producto && p.Stock != null);
            if (delta < 0 && !config.TpvPermitirStockNegativo)
            {
                var gestionaStock = await query.AnyAsync();
                if (!gestionaStock) continue;
                var filas = await query.Where(p => p.Stock >= unidades)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + delta));
                if (filas == 0)
                {
                    var nombre = await context.Productos.Where(p => p.Id == productoId).Select(p => p.Nombre).FirstAsync();
                    throw new InvalidOperationException($"Stock insuficiente de {nombre}.");
                }
            }
            else
            {
                await query.ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + delta));
            }
        }
    }

    public async Task<List<TicketTpv>> BuscarTicketsAsync(FiltroTicketsTpv filtro)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.TicketsTpv.AsNoTracking()
            .Include(t => t.Cliente)
            .Include(t => t.Terminal)
            .Include(t => t.Usuario)
            .Include(t => t.Factura)
            .Where(t => t.EmpresaId == empresaId);

        if (filtro.Desde.HasValue) query = query.Where(t => t.Fecha >= filtro.Desde.Value.Date);
        if (filtro.Hasta.HasValue) query = query.Where(t => t.Fecha < filtro.Hasta.Value.Date.AddDays(1));
        if (filtro.TerminalId.HasValue) query = query.Where(t => t.TerminalId == filtro.TerminalId);
        if (filtro.UsuarioId.HasValue) query = query.Where(t => t.UsuarioId == filtro.UsuarioId);
        if (filtro.ClienteId.HasValue) query = query.Where(t => t.ClienteId == filtro.ClienteId);
        if (filtro.Estado.HasValue) query = query.Where(t => t.Estado == filtro.Estado);
        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim();
            query = query.Where(t => (t.Numero != null && t.Numero.Contains(texto))
                || (t.Cliente != null && ((t.Cliente.Nombre != null && t.Cliente.Nombre.Contains(texto))
                    || (t.Cliente.NombreEmpresa != null && t.Cliente.NombreEmpresa.Contains(texto))
                    || t.Cliente.NIF.Contains(texto))));
        }

        return await query.OrderByDescending(t => t.Fecha).ThenByDescending(t => t.Id)
            .Take(Math.Clamp(filtro.Maximo, 1, 1000)).ToListAsync();
    }

    public async Task<Dictionary<string, decimal>> ObtenerCantidadesDevueltasAsync(int ticketId)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        return await CantidadesDevueltasAsync(context, empresaId, ticketId);
    }

    private static async Task<Dictionary<string, decimal>> CantidadesDevueltasAsync(ApplicationDbContext context, int empresaId, int ticketId)
    {
        var lineas = await context.LineasDocumento.AsNoTracking()
            .Where(l => l.TicketTpv != null && l.TicketTpv.EmpresaId == empresaId && l.TicketTpv.TicketOriginalId == ticketId
                && (l.TicketTpv.Estado == EstadoTicketTpv.Cobrado || l.TicketTpv.Estado == EstadoTicketTpv.Facturado))
            .ToListAsync();
        return lineas.GroupBy(ITpvService.ClaveLinea).ToDictionary(g => g.Key, g => Math.Abs(g.Sum(l => l.Cantidad)));
    }

    public async Task<TicketTpv> DevolverAsync(int ticketOriginalId, List<LineaDevolucionTpv> lineas, int terminalId, int formaPagoId, string motivo)
    {
        var (empresaId, usuarioId) = await ValidarAccesoAsync();
        if (string.IsNullOrWhiteSpace(motivo))
            throw new InvalidOperationException("Indique el motivo de la devolución.");
        motivo = motivo.Trim()[..Math.Min(motivo.Trim().Length, 500)];
        lineas = lineas.Where(l => l.Cantidad > 0).ToList();
        if (lineas.Count == 0)
            throw new InvalidOperationException("Seleccione al menos una línea a devolver.");

        using var context = await _contextFactory.CreateDbContextAsync();
        var original = await context.TicketsTpv.AsNoTracking()
            .Include(t => t.Lineas)
            .Include(t => t.Factura)
            .FirstOrDefaultAsync(t => t.Id == ticketOriginalId && t.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("Ticket no encontrado.");
        if (original.EsDevolucion || original.Estado is not (EstadoTicketTpv.Cobrado or EstadoTicketTpv.Facturado))
            throw new InvalidOperationException("Solo se pueden devolver tickets de venta cobrados.");
        var sesion = await context.TpvSesionesCaja.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TerminalId == terminalId && s.EmpresaId == empresaId && s.Estado == EstadoSesionCaja.Abierta)
            ?? throw new InvalidOperationException("Abra la caja del terminal antes de registrar la devolución.");
        var forma = await context.FormasPagoPersonalizadas.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == formaPagoId && f.EmpresaId == empresaId && f.Activo)
            ?? throw new InvalidOperationException("Forma de pago de reembolso no válida.");

        var devuelto = await CantidadesDevueltasAsync(context, empresaId, ticketOriginalId);
        var lineasDevolucion = new List<LineaDocumento>();
        foreach (var solicitud in lineas)
        {
            var lineaOriginal = original.Lineas.FirstOrDefault(l => l.Id == solicitud.LineaId)
                ?? throw new InvalidOperationException("La línea no pertenece al ticket.");
            var clave = ITpvService.ClaveLinea(lineaOriginal);
            var vendido = original.Lineas.Where(l => ITpvService.ClaveLinea(l) == clave).Sum(l => l.Cantidad);
            var yaDevuelto = devuelto.GetValueOrDefault(clave) + lineasDevolucion.Where(l => ITpvService.ClaveLinea(l) == clave).Sum(l => -l.Cantidad);
            if (solicitud.Cantidad > vendido - yaDevuelto + 0.001m)
                throw new InvalidOperationException($"No se pueden devolver más unidades de \"{lineaOriginal.Descripcion}\" de las vendidas.");
            lineasDevolucion.Add(new LineaDocumento
            {
                ProductoId = lineaOriginal.ProductoId,
                Descripcion = lineaOriginal.Descripcion,
                Cantidad = -Math.Round(solicitud.Cantidad, 2),
                PrecioUnitario = lineaOriginal.PrecioUnitario,
                IVA = lineaOriginal.IVA,
                Descuento = lineaOriginal.Descuento
            });
        }

        var devolucion = new TicketTpv
        {
            EmpresaId = empresaId,
            TipoTicket = TipoTicketTpv.Devolucion,
            TicketOriginalId = original.Id,
            TerminalId = terminalId,
            SesionCajaId = sesion.Id,
            UsuarioId = usuarioId,
            ClienteId = original.ClienteId,
            DescuentoGlobal = original.DescuentoGlobal,
            Fecha = DateTime.Now,
            Notas = motivo,
            Lineas = lineasDevolucion
        };
        devolucion.CalcularTotales();
        devolucion.Pagos.Add(new PagoTicketTpv
        {
            FormaPagoPersonalizadaId = forma.Id,
            EsEfectivo = EsEfectivo(forma),
            Importe = devolucion.Total
        });
        devolucion.Entregado = devolucion.Total;

        // El abono se crea antes de bloquear la configuración: su numeración usa su propia transacción
        Factura? abono = null;
        if (original.Estado == EstadoTicketTpv.Facturado && original.Factura != null)
        {
            abono = await CrearAbonoDevolucionAsync(original.Factura, devolucion, forma.Id, motivo);
            devolucion.FacturaId = abono.Id;
        }

        try
        {
            await using var transaccion = await context.Database.BeginTransactionAsync();
            var config = await BloquearConfiguracionAsync(context, empresaId);
            devolucion.Numero = await SiguienteNumeroAsync(context, config, empresaId, devolucion: true, terminalId);
            devolucion.FechaCobro = DateTime.Now;
            devolucion.Estado = abono != null ? EstadoTicketTpv.Facturado : EstadoTicketTpv.Cobrado;
            devolucion.Huella = await CalcularHuellaAsync(context, empresaId, config, devolucion);
            context.TicketsTpv.Add(devolucion);
            await AjustarStockAsync(context, empresaId, config, lineasDevolucion, signo: +1);
            await context.SaveChangesAsync();
            await transaccion.CommitAsync();
        }
        catch
        {
            if (abono != null)
                await _facturaService.EliminarAsync(abono.Id);
            throw;
        }

        return (await ObtenerTicketAsync(devolucion.Id))!;
    }

    private async Task<Factura> CrearAbonoDevolucionAsync(Factura facturaOriginal, TicketTpv devolucion, int formaPagoId, string motivo)
    {
        var factor = 1 - devolucion.DescuentoGlobal / 100m;
        var abono = new Factura
        {
            TipoFactura = TipoFactura.Venta,
            ClaseFactura = ClaseFactura.Rectificativa,
            FacturaOriginalId = facturaOriginal.Id,
            ClienteId = facturaOriginal.ClienteId,
            FechaEmision = DateTime.Now,
            FechaOperacion = DateTime.Now,
            MotivoRectificacion = $"Devolución TPV: {motivo}",
            Estado = EstadoFactura.Pagada,
            FechaPago = DateTime.Now,
            FormaPagoPersonalizadaId = formaPagoId,
            Observaciones = "Generado automáticamente desde una devolución del TPV",
            Lineas = devolucion.Lineas.Select(l => new LineaDocumento
            {
                ProductoId = l.ProductoId,
                Descripcion = l.Descripcion,
                Cantidad = l.Cantidad,
                PrecioUnitario = l.PrecioUnitario,
                IVA = l.IVA,
                Descuento = DescuentoCombinado(l.Descuento, factor)
            }).ToList()
        };
        return await _facturaService.CrearAsync(abono);
    }

    private static decimal DescuentoCombinado(decimal descuentoLinea, decimal factorGlobal) =>
        Math.Round(100m - (100m - descuentoLinea) * factorGlobal, 2);

    #endregion

    #region Presupuestos

    public async Task<List<Presupuesto>> ObtenerPresupuestosCobrablesAsync()
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var usados = context.TicketsTpv.Where(t => t.EmpresaId == empresaId && t.PresupuestoOrigenId != null
            && (t.Estado == EstadoTicketTpv.Cobrado || t.Estado == EstadoTicketTpv.Facturado))
            .Select(t => t.PresupuestoOrigenId!.Value);
        return await context.Presupuestos.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Lineas)
            .Where(p => p.EmpresaId == empresaId
                && (p.Estado == EstadoPresupuesto.Aceptado || p.Estado == EstadoPresupuesto.Enviado)
                && !usados.Contains(p.Id))
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();
    }

    public async Task<TicketTpv> CrearTicketDesdePresupuestoAsync(int presupuestoId, int terminalId)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var presupuesto = await context.Presupuestos.AsNoTracking()
            .Include(p => p.Lineas).ThenInclude(l => l.Producto)
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == presupuestoId && p.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("Presupuesto no encontrado.");
        if (presupuesto.Estado is not (EstadoPresupuesto.Aceptado or EstadoPresupuesto.Enviado))
            throw new InvalidOperationException("Solo se pueden cobrar presupuestos aceptados o enviados.");
        await ValidarPresupuestoLibreAsync(context, empresaId, presupuestoId, 0);

        var ivaPorDefecto = await context.ConfiguracionEmpresa.Where(c => c.EmpresaId == empresaId)
            .Select(c => (decimal?)c.IVAPorDefecto).FirstOrDefaultAsync() ?? 21m;

        // Los presupuestos se guardan sin IVA; en el ticket se aplica el IVA del producto o el de la empresa
        var ticket = new TicketTpv
        {
            EmpresaId = empresaId,
            TerminalId = terminalId,
            ClienteId = presupuesto.ClienteId,
            Cliente = presupuesto.Cliente,
            PresupuestoOrigenId = presupuesto.Id,
            Notas = $"Presupuesto {presupuesto.Numero}",
            Lineas = presupuesto.Lineas.Where(l => l.Cantidad > 0).Select(l => new LineaDocumento
            {
                ProductoId = l.ProductoId,
                Producto = l.Producto,
                Descripcion = l.Descripcion,
                Cantidad = l.Cantidad,
                PrecioUnitario = l.PrecioUnitario,
                Descuento = l.Descuento,
                IVA = l.Producto?.IVA ?? ivaPorDefecto
            }).ToList()
        };
        ticket.CalcularTotales();
        return ticket;
    }

    #endregion

    #region Facturas

    public Task<Factura> ConvertirTicketEnFacturaAsync(int ticketId, int clienteId) =>
        FacturarTicketsAsync(clienteId, new List<int> { ticketId });

    public async Task<Factura> FacturarTicketsAsync(int clienteId, List<int> ticketIds)
    {
        var (empresaId, _) = await ValidarAccesoAsync();
        ticketIds = ticketIds.Distinct().ToList();
        if (ticketIds.Count == 0)
            throw new InvalidOperationException("Seleccione al menos un ticket.");

        using var context = await _contextFactory.CreateDbContextAsync();
        var cliente = await context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.Id == clienteId && c.EmpresaId == empresaId)
            ?? throw new InvalidOperationException("Cliente no encontrado.");
        if (string.IsNullOrWhiteSpace(cliente.NIF))
            throw new InvalidOperationException("El cliente necesita NIF para emitir una factura completa.");

        await using var transaccion = await context.Database.BeginTransactionAsync();
        var tickets = new List<TicketTpv>();
        foreach (var id in ticketIds.OrderBy(i => i))
        {
            var bloqueados = await context.TicketsTpv
                .FromSqlInterpolated($"SELECT * FROM TicketsTpv WHERE Id = {id} AND EmpresaId = {empresaId} FOR UPDATE")
                .ToListAsync();
            var ticket = bloqueados.SingleOrDefault() ?? throw new InvalidOperationException("Ticket no encontrado.");
            if (ticket.EsDevolucion || ticket.Estado != EstadoTicketTpv.Cobrado || ticket.FacturaId.HasValue)
                throw new InvalidOperationException($"El ticket {ticket.Numero} no se puede facturar (debe estar cobrado y sin factura).");
            if (ticket.ClienteId.HasValue && ticket.ClienteId != clienteId)
            {
                var esGenerico = await context.TpvTerminales.AnyAsync(t => t.EmpresaId == empresaId && t.ClienteGenericoId == ticket.ClienteId);
                if (!esGenerico)
                    throw new InvalidOperationException($"El ticket {ticket.Numero} está asignado a otro cliente.");
            }
            if (await context.TicketsTpv.AnyAsync(t => t.TicketOriginalId == ticket.Id
                    && (t.Estado == EstadoTicketTpv.Cobrado || t.Estado == EstadoTicketTpv.Facturado)))
                throw new InvalidOperationException($"El ticket {ticket.Numero} tiene devoluciones y no se puede canjear por factura.");
            tickets.Add(ticket);
        }

        var ids = tickets.Select(t => t.Id).ToList();
        var lineas = await context.LineasDocumento.AsNoTracking().Where(l => l.TicketTpvId != null && ids.Contains(l.TicketTpvId.Value)).ToListAsync();
        var pagos = await context.PagosTicketTpv.AsNoTracking().Where(p => ids.Contains(p.TicketTpvId)).ToListAsync();

        var factura = new Factura
        {
            TipoFactura = TipoFactura.Venta,
            ClaseFactura = ClaseFactura.Original,
            ClienteId = clienteId,
            FechaEmision = DateTime.Now,
            FechaOperacion = tickets.Min(t => t.FechaCobro) ?? DateTime.Now,
            FechaPago = tickets.Max(t => t.FechaCobro) ?? DateTime.Now,
            Estado = EstadoFactura.Pagada,
            PresupuestoId = tickets.Count == 1 ? tickets[0].PresupuestoOrigenId : null,
            FormaPagoPersonalizadaId = pagos.FirstOrDefault()?.FormaPagoPersonalizadaId,
            Observaciones = $"Canje de factura simplificada: {string.Join(", ", tickets.Select(t => t.Numero))}",
            Lineas = tickets.SelectMany(t =>
            {
                var factor = 1 - t.DescuentoGlobal / 100m;
                return lineas.Where(l => l.TicketTpvId == t.Id).Select(l => new LineaDocumento
                {
                    ProductoId = l.ProductoId,
                    Descripcion = l.Descripcion,
                    Cantidad = l.Cantidad,
                    PrecioUnitario = l.PrecioUnitario,
                    IVA = l.IVA,
                    Descuento = DescuentoCombinado(l.Descuento, factor)
                });
            }).ToList(),
            Pagos = tickets.SelectMany(t => pagos.Where(p => p.TicketTpvId == t.Id).Select(p => new PagoFactura
            {
                FechaPago = t.FechaCobro ?? DateTime.Now,
                Importe = p.Importe,
                FormaPagoPersonalizadaId = p.FormaPagoPersonalizadaId,
                Referencia = t.Numero,
                Notas = "Cobrado en TPV",
                FechaRegistro = DateTime.Now
            })).ToList()
        };

        var creada = await _facturaService.CrearAsync(factura);
        try
        {
            foreach (var ticket in tickets)
            {
                ticket.FacturaId = creada.Id;
                ticket.Estado = EstadoTicketTpv.Facturado;
            }
            await context.SaveChangesAsync();
            await transaccion.CommitAsync();
        }
        catch
        {
            await _facturaService.EliminarAsync(creada.Id);
            throw;
        }
        return creada;
    }

    public async Task<Dictionary<int, string>> ObtenerTicketsPorFacturaAsync(IEnumerable<int> facturaIds)
    {
        var ids = facturaIds.Distinct().ToList();
        if (ids.Count == 0) return new();
        using var context = await _contextFactory.CreateDbContextAsync();
        var filas = await context.TicketsTpv.AsNoTracking()
            .Where(t => t.FacturaId != null && ids.Contains(t.FacturaId.Value) && t.Numero != null)
            .Select(t => new { FacturaId = t.FacturaId!.Value, t.Numero })
            .ToListAsync();
        return filas.GroupBy(f => f.FacturaId).ToDictionary(g => g.Key, g => string.Join(", ", g.Select(f => f.Numero)));
    }

    #endregion
}
