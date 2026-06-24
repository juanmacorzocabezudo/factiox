using System.ComponentModel.DataAnnotations.Schema;

namespace FactioX.Models;

public enum EstadoPresupuesto
{
    Borrador,
    Enviado,
    Aceptado,
    Rechazado,
    Caducado
}

public class Presupuesto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
    public DateTime? FechaValidez { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public List<LineaDocumento> Lineas { get; set; } = new();
    public EstadoPresupuesto Estado { get; set; } = EstadoPresupuesto.Borrador;
    public string? Notas { get; set; }
    public string? CondicionesPago { get; set; }
    
    // Multi-tenant
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    
    // Propiedades calculadas (no mapeadas a la BD)
    [NotMapped]
    public decimal BaseImponible => Lineas?.Sum(l => l.Subtotal) ?? 0;
    
    [NotMapped]
    public decimal TotalIVA => Lineas?.Sum(l => l.ImporteIVA) ?? 0;
    
    [NotMapped]
    public decimal Total => BaseImponible; // IVA no incluido en presupuestos
}
