using System.ComponentModel.DataAnnotations.Schema;

namespace FactioX.Models;

public class LineaDocumento
{
    public int Id { get; set; }
    
    // Relación con Factura
    public int? FacturaId { get; set; }
    public Factura? Factura { get; set; }
    
    // Relación con Presupuesto
    public int? PresupuestoId { get; set; }
    public Presupuesto? Presupuesto { get; set; }
    
    // Relación con Producto
    public int? ProductoId { get; set; }
    public Producto? Producto { get; set; }
    
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal IVA { get; set; }
    public decimal Descuento { get; set; } = 0; // Porcentaje de descuento
    
    // Campos calculados almacenados en BD
    public decimal Subtotal { get; set; }
    public decimal ImporteIVA { get; set; }
    public decimal Total { get; set; }
    
    // Método para calcular valores
    public void CalcularImportes()
    {
        Subtotal = Cantidad * PrecioUnitario * (1 - Descuento / 100);
        ImporteIVA = Subtotal * (IVA / 100);
        Total = Subtotal + ImporteIVA;
    }
}
