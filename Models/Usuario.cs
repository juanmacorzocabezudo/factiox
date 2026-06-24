using System.ComponentModel.DataAnnotations;

namespace FactioX.Models;

public class Usuario
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [MaxLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres")]
    public string Username { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
    
    // Campo para almacenar contraseña en texto plano (solo visible para SuperAdmin)
    public string? PasswordTextoPlano { get; set; }
    
    [Required(ErrorMessage = "El nombre completo es requerido")]
    [MaxLength(200, ErrorMessage = "El nombre completo no puede exceder los 200 caracteres")]
    public string NombreCompleto { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [MaxLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El rol es requerido")]
    [MaxLength(50)]
    public string Rol { get; set; } = "Usuario"; // "SuperAdministrador", "Administrador" o "Usuario"
    
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    // Fecha de caducidad (para accesos de prueba)
    public DateTime? FechaCaducidad { get; set; }
    
    // Indica si es un usuario de prueba
    public bool EsAccesoPrueba { get; set; } = false;
    
    // Multi-tenant: null para SuperAdministrador, requerido para otros roles (excepto Asesoría)
    public int? EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
    
    // Relación many-to-many con Empresa (para Asesorías)
    public ICollection<UsuarioEmpresa> EmpresasAsociadas { get; set; } = new List<UsuarioEmpresa>();
}

public static class Roles
{
    public const string SuperAdministrador = "SuperAdministrador";
    public const string Administrador = "Administrador";
    public const string Usuario = "Usuario";
    public const string Asesoria = "Asesoría";
}
