using System.ComponentModel.DataAnnotations;

namespace FactioX.Models;

/// <summary>
/// Tabla intermedia para la relación many-to-many entre Usuario y Empresa
/// Permite que un usuario (especialmente Asesorías) tenga acceso a múltiples empresas
/// </summary>
public class UsuarioEmpresa
{
    public int Id { get; set; }
    
    [Required]
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;
    
    [Required]
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    
    public DateTime FechaAsociacion { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
}
