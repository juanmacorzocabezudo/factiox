using System.ComponentModel.DataAnnotations;

namespace FactioX.Models;

public class Cliente
{
    public int Id { get; set; }
    
    [MaxLength(200)]
    public string? Nombre { get; set; }
    
    [MaxLength(200)]
    public string? Apellidos { get; set; }
    
    [MaxLength(200)]
    public string? NombreEmpresa { get; set; }
    
    public TipoCliente Tipo { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string NIF { get; set; } = string.Empty;
    
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    
    [MaxLength(10)]
    public string? CodigoPostal { get; set; }
    
    public string? Provincia { get; set; }
    
    [MaxLength(3)]
    public string Pais { get; set; } = "ESP";
    
    [Required]
    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Fax { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? EmailSecundario { get; set; }
    
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; } = true;
    
    // Datos FacturaE
    public bool ClienteFacturaE { get; set; } = false;
    public TipoPersona TipoPersona { get; set; } = TipoPersona.Juridica;
    public TipoResidencia TipoResidencia { get; set; } = TipoResidencia.Residente;
    public string? PersonaContacto { get; set; }
    public string? CodigoINE { get; set; }
    
    // Datos Financieros
    [MaxLength(34)]
    public string? CuentaBancaria { get; set; }
    [MaxLength(11)]
    public string? BIC { get; set; }
    [MaxLength(35)]
    public string? MandateReference { get; set; }
    public DateTime? MandateSignatureDate { get; set; }
    public FormaPago? FormaPago { get; set; }
    public decimal? LimiteCredito { get; set; }
    public int? DiasPago { get; set; }
    public string? Contacto { get; set; }
    
    // Facturación Masiva / Recurrente
    public int? ProductoRecurrenteId { get; set; }
    public Producto? ProductoRecurrente { get; set; }
    public string? ConceptoRecurrente { get; set; }
    public decimal? ImporteRecurrente { get; set; }
    public PeriodicidadFacturacion? Periodicidad { get; set; }
    public DateTime? UltimaFacturacion { get; set; }
    public bool FacturacionMasivaActiva { get; set; } = false;
    
    // Multi-tenant
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    
    // Navegación
    public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}

public enum TipoCliente
{
    Particular,
    Empresa,
    Agencia
}

public enum TipoPersona
{
    [Display(Name = "Física")]
    Fisica,
    [Display(Name = "Jurídica")]
    Juridica
}

public enum TipoResidencia
{
    [Display(Name = "Residente")]
    Residente,
    [Display(Name = "Unión Europea")]
    UnionEuropea,
    [Display(Name = "Extranjero")]
    Extranjero
}

public enum PeriodicidadFacturacion
{
    [Display(Name = "Mensual")]
    Mensual,
    [Display(Name = "Bimensual")]
    Bimensual,
    [Display(Name = "Trimestral")]
    Trimestral,
    [Display(Name = "Semestral")]
    Semestral,
    [Display(Name = "Anual")]
    Anual,
    [Display(Name = "Puntual")]
    Puntual
}
