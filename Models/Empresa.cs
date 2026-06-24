using System.ComponentModel.DataAnnotations;

namespace FactioX.Models;

public class Empresa
{
    public int Id { get; set; }
    
    // Datos básicos
    [Required(ErrorMessage = "El nombre comercial es requerido")]
    [MaxLength(200)]
    public string NombreComercial { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El slug es requerido")]
    [MaxLength(100)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "El slug solo puede contener letras minúsculas, números y guiones")]
    public string Slug { get; set; } = string.Empty; // URL amigable
    
    // Estado
    public bool Activa { get; set; } = true;
    public DateTime FechaAlta { get; set; } = DateTime.Now;
    public DateTime? FechaBaja { get; set; }
    
    // Plan/Suscripción (para control de límites)
    [Required]
    [MaxLength(50)]
    public string PlanSuscripcion { get; set; } = "Basico"; // Basico, Profesional, Empresarial
    public int? MaxUsuarios { get; set; } = 5;
    
    // Relaciones
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
    public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    public ICollection<Proveedor> Proveedores { get; set; } = new List<Proveedor>();
    public ConfiguracionEmpresa? ConfiguracionEmpresa { get; set; }
    
    // Relación many-to-many con Usuario (para Asesorías)
    public ICollection<UsuarioEmpresa> UsuariosAsociados { get; set; } = new List<UsuarioEmpresa>();
}
