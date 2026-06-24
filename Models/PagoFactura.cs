using System.ComponentModel.DataAnnotations;

namespace FactioX.Models;

public class PagoFactura
{
    public int Id { get; set; }
    
    [Required]
    public int FacturaId { get; set; }
    public Factura Factura { get; set; } = null!;
    
    [Required]
    public DateTime FechaPago { get; set; } = DateTime.Now;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor que 0")]
    public decimal Importe { get; set; }
    
    // Forma de pago (enum legacy - mantener para compatibilidad)
    public FormaPago? FormaPago { get; set; }
    
    // Forma de pago personalizada (heredada de la factura)
    public int? FormaPagoPersonalizadaId { get; set; }
    public FormaPagoPersonalizada? FormaPagoPersonalizada { get; set; }
    
    [MaxLength(500)]
    public string? Notas { get; set; }
    
    [MaxLength(100)]
    public string? Referencia { get; set; }
    
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}
