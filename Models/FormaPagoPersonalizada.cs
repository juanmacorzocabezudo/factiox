using System.ComponentModel.DataAnnotations;

namespace FactioX.Models;

public class FormaPagoPersonalizada
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Descripcion { get; set; }
    
    public bool Activo { get; set; } = true;
    
    // Indica si es una forma de pago predefinida del sistema (no se puede eliminar)
    public bool EsPredefinido { get; set; } = false;
    
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    // Multi-tenant
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
