using System.ComponentModel.DataAnnotations;

namespace FactioX.Models;

public class Proveedor
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? NIF { get; set; }
    
    public string? Direccion { get; set; }
    
    [MaxLength(10)]
    public string? CodigoPostal { get; set; }
    
    public string? Ciudad { get; set; }
    public string? Provincia { get; set; }
    
    [MaxLength(3)]
    public string Pais { get; set; } = "ESP";
    
    [MaxLength(100)]
    public string? Email { get; set; }
    
    [MaxLength(20)]
    public string? Telefono { get; set; }
    
    [MaxLength(20)]
    public string? Fax { get; set; }
    
    public string? Web { get; set; }
    public string? PersonaContacto { get; set; }
    
    // Datos Financieros
    [MaxLength(34)]
    public string? CuentaBancaria { get; set; }
    
    public FormaPago? FormaPago { get; set; }
    public int? DiasPago { get; set; }
    public string? Observaciones { get; set; }
    public bool Activo { get; set; } = true;
    
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    // Multi-tenant
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
