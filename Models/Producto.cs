using System.ComponentModel.DataAnnotations;

namespace FactioX.Models;

public enum TipoProducto
{
    Producto,
    Servicio
}

public enum UnidadMedida
{
    Unidad,
    Kg,
    Litro,
    Metro,
    M2,
    M3,
    Hora,
    Dia,
    Caja,
    Paquete
}

public class Producto
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? Referencia { get; set; }
    
    [MaxLength(100)]
    public string? CodigoBarras { get; set; }
    
    public string? Descripcion { get; set; }
    
    [MaxLength(100)]
    public string? Categoria { get; set; }
    
    [MaxLength(100)]
    public string? Marca { get; set; }
    
    public TipoProducto Tipo { get; set; } = TipoProducto.Producto;
    
    public UnidadMedida UnidadMedida { get; set; } = UnidadMedida.Unidad;
    
    // Precios y costes
    public decimal PrecioCompra { get; set; } = 0;
    public decimal PrecioUnitario { get; set; }
    public decimal IVA { get; set; } = 21; // IVA por defecto 21%
    public decimal? Descuento { get; set; } // Descuento en porcentaje
    
    // Stock (solo para productos, no servicios)
    public int? Stock { get; set; }
    public int? StockMinimo { get; set; }
    public int? StockMaximo { get; set; }
    
    // Dimensiones y peso
    public decimal? Peso { get; set; } // en kg
    public decimal? Alto { get; set; } // en cm
    public decimal? Ancho { get; set; } // en cm
    public decimal? Largo { get; set; } // en cm
    
    // Información adicional
    public string? Imagen { get; set; }
    public string? Observaciones { get; set; }
    
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaUltimaActualizacion { get; set; }
    
    // Multi-tenant
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
