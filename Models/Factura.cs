using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactioX.Models;

public class Factura
{
    public int Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public TipoFactura TipoFactura { get; set; } = TipoFactura.Venta;
    public DateTime FechaEmision { get; set; }
    public DateTime? FechaOperacion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int? ProveedorId { get; set; }
    public Proveedor? Proveedor { get; set; }
    public decimal BaseImponible { get; set; }
    public decimal PorcentajeIVA { get; set; } = 21m;
    public decimal ImporteIVA { get; set; }
    public decimal CargosAdicionales { get; set; } = 0m;
    public decimal PorcentajeRetencion { get; set; } = 0m;
    public decimal ImporteRetencion { get; set; } = 0m;
    public decimal Total { get; set; }
    public EstadoFactura Estado { get; set; }
    public string? Concepto { get; set; }
    public decimal ImporteConcepto { get; set; } = 0m;
    public string? ConceptoCargos { get; set; }
    public string? Observaciones { get; set; }
    public DateTime? FechaPago { get; set; }
    public FormaPago? FormaPago { get; set; }
    
    // Forma de pago personalizada
    public int? FormaPagoPersonalizadaId { get; set; }
    public FormaPagoPersonalizada? FormaPagoPersonalizada { get; set; }
    
    // Datos FacturaE
    public string? LugarExpedicion { get; set; }
    public string? CodigoPostalExpedicion { get; set; }
    public DateTime? FechaPeriodoInicio { get; set; }
    public DateTime? FechaPeriodoFin { get; set; }
    public ClaseFactura ClaseFactura { get; set; } = ClaseFactura.Original;
    public int? FacturaOriginalId { get; set; }
    public string? NumeroFacturaOriginal { get; set; }
    public string? MotivoRectificacion { get; set; }
    public decimal DescuentosGenerales { get; set; } = 0m;
    public decimal RecargosGenerales { get; set; } = 0m;
    
    // Multi-tenant
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    
    // Navegación
    public List<LineaDocumento> Lineas { get; set; } = new List<LineaDocumento>();
    public int? PresupuestoId { get; set; }
    public Presupuesto? Presupuesto { get; set; }
    
    // Pagos parciales
    public List<PagoFactura> Pagos { get; set; } = new List<PagoFactura>();
    
    // Documento adjunto (para facturas de compra)
    public string? DocumentoAdjunto { get; set; }
    public string? NombreDocumento { get; set; }
    public string? TipoDocumento { get; set; }
    
    // Propiedades calculadas (no mapeadas a la BD)
    [NotMapped]
    public bool EsAbono => ClaseFactura == ClaseFactura.Rectificativa;

    [NotMapped]
    public decimal TotalIVA => Lineas?.Sum(l => l.ImporteIVA) ?? 0;
    
    [NotMapped]
    public decimal Subtotal => Lineas?.Sum(l => l.Subtotal) ?? 0;
    
    [NotMapped]
    public decimal TotalPagado => Pagos?.Sum(p => p.Importe) ?? 0;
    
    [NotMapped]
    public decimal PendientePago => Total - TotalPagado;
    
    [NotMapped]
    public bool EstaCompletamentePagada => PendientePago <= 0.01m; // Tolerancia de 1 céntimo
    
    [NotMapped]
    public bool TienePagosParciales => Pagos?.Any() == true && !EstaCompletamentePagada;

    public void PrepararImportesAbono()
    {
        if (!EsAbono) return;

        foreach (var linea in Lineas)
        {
            linea.Cantidad = -Math.Abs(linea.Cantidad);
            linea.PrecioUnitario = Math.Abs(linea.PrecioUnitario);
        }
        DescuentosGenerales = -Math.Abs(DescuentosGenerales);
        RecargosGenerales = -Math.Abs(RecargosGenerales);
        CargosAdicionales = -Math.Abs(CargosAdicionales);
        ImporteRetencion = -Math.Abs(ImporteRetencion);
        ImporteConcepto = -Math.Abs(ImporteConcepto);
    }

    public Factura CrearBorradorAbono()
    {
        if (Id <= 0 || EsAbono)
            throw new InvalidOperationException("Seleccione una factura original guardada para rectificar.");

        var abono = new Factura
        {
            EmpresaId = EmpresaId,
            TipoFactura = TipoFactura,
            ClaseFactura = ClaseFactura.Rectificativa,
            FacturaOriginalId = Id,
            NumeroFacturaOriginal = NumeroFactura,
            FechaEmision = DateTime.Now,
            FechaOperacion = FechaOperacion ?? FechaEmision,
            Estado = EstadoFactura.Borrador,
            ClienteId = ClienteId,
            Cliente = Cliente,
            ProveedorId = ProveedorId,
            Proveedor = Proveedor,
            FormaPago = FormaPago,
            FormaPagoPersonalizadaId = FormaPagoPersonalizadaId,
            Concepto = Concepto,
            ImporteConcepto = ImporteConcepto,
            ConceptoCargos = ConceptoCargos,
            PorcentajeIVA = PorcentajeIVA,
            DescuentosGenerales = DescuentosGenerales,
            RecargosGenerales = RecargosGenerales,
            CargosAdicionales = CargosAdicionales,
            PorcentajeRetencion = PorcentajeRetencion,
            ImporteRetencion = ImporteRetencion,
            LugarExpedicion = LugarExpedicion,
            CodigoPostalExpedicion = CodigoPostalExpedicion,
            FechaPeriodoInicio = FechaPeriodoInicio,
            FechaPeriodoFin = FechaPeriodoFin,
            Lineas = Lineas.Select(linea => new LineaDocumento
            {
                ProductoId = linea.ProductoId,
                Descripcion = linea.Descripcion,
                Cantidad = linea.Cantidad,
                PrecioUnitario = linea.PrecioUnitario,
                IVA = linea.IVA,
                Descuento = linea.Descuento
            }).ToList()
        };
        abono.PrepararImportesAbono();
        foreach (var linea in abono.Lineas) linea.CalcularImportes();
        abono.BaseImponible = abono.Lineas.Sum(linea => linea.Subtotal) - abono.DescuentosGenerales + abono.RecargosGenerales;
        abono.ImporteIVA = abono.Lineas.Sum(linea => linea.ImporteIVA);
        abono.Total = abono.BaseImponible + abono.ImporteIVA + abono.CargosAdicionales - abono.ImporteRetencion;
        return abono;
    }
}

public enum TipoFactura
{
    Venta,
    Compra
}

public enum EstadoFactura
{
    Borrador,
    Emitida,
    Enviada,
    Pagada,
    Vencida,
    Cancelada
}

public enum FormaPago
{
    Efectivo,
    Transferencia,
    [Display(Name = "Tarjeta Crédito")]
    TarjetaCredito,
    [Display(Name = "Tarjeta Débito")]
    TarjetaDebito,
    Bizum,
    [Display(Name = "Domiciliación")]
    Domiciliacion
}

public enum ClaseFactura
{
    [Display(Name = "Original")]
    Original,
    [Display(Name = "Rectificativa")]
    Rectificativa,
    [Display(Name = "Recapitulativa")]
    Recapitulativa
}
