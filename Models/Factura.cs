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
