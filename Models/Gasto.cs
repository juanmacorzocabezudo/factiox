using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FactioX.Models;

public class Gasto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "La fecha es obligatoria")]
    public DateTime Fecha { get; set; } = DateTime.Now;
    
    [Required(ErrorMessage = "El concepto es obligatorio")]
    [StringLength(500, ErrorMessage = "El concepto no puede exceder los 500 caracteres")]
    public string Concepto { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "La categoría no puede exceder los 100 caracteres")]
    public string? Categoria { get; set; }
    
    [Required(ErrorMessage = "El importe es obligatorio")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, 999999999.99, ErrorMessage = "El importe debe ser mayor que 0")]
    public decimal Importe { get; set; }
    
    [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder los 1000 caracteres")]
    public string? Observaciones { get; set; }
    
    // Multi-tenant
    [Required]
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    
    // Auditoría
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
}
