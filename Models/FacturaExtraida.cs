namespace FactioX.Models;

/// <summary>
/// DTO para datos extraídos de una factura mediante OCR
/// </summary>
public class FacturaExtraida
{
    // Datos del proveedor
    public string NombreProveedor { get; set; } = string.Empty;
    public string? NifProveedor { get; set; }
    public string? DireccionProveedor { get; set; }
    public string? CodigoPostalProveedor { get; set; }
    public string? CiudadProveedor { get; set; }
    public string? ProvinciaProveedor { get; set; }
    public string? TelefonoProveedor { get; set; }
    public string? EmailProveedor { get; set; }
    
    // Datos de la factura
    public string? NumeroFactura { get; set; }
    public DateTime? FechaEmision { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    
    // Líneas de factura
    public List<LineaFacturaExtraida> Lineas { get; set; } = new();
    
    // Totales
    public decimal? BaseImponible { get; set; }
    public decimal? PorcentajeIVA { get; set; }
    public decimal? ImporteIVA { get; set; }
    public decimal? Total { get; set; }
    
    // Datos adicionales
    public string? FormaPago { get; set; }
    public string? Observaciones { get; set; }
    
    // Confianza de la extracción (0-100)
    public int Confianza { get; set; } = 0;
    
    // Errores o advertencias
    public List<string> Advertencias { get; set; } = new();
}

/// <summary>
/// Línea de factura extraída
/// </summary>
public class LineaFacturaExtraida
{
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal? Descuento { get; set; }
    public decimal? IVA { get; set; }
    public decimal Total { get; set; }
}
