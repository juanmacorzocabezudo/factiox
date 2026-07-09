namespace FactioX.Models;

public class ConfiguracionEmpresa
{
    public int Id { get; set; }
    public string NombreEmpresa { get; set; } = string.Empty;
    public string NIF { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public string Provincia { get; set; } = string.Empty;
    public string Pais { get; set; } = "ESP";
    public string Telefono { get; set; } = string.Empty;
    public string? Fax { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Web { get; set; }
    public string? IBAN { get; set; }
    public string? BIC { get; set; }
    public string? CreditorId { get; set; }
    public string? LogoRuta { get; set; }
    
    // Datos FacturaE
    public TipoPersona TipoPersona { get; set; } = TipoPersona.Juridica;
    public TipoResidencia TipoResidencia { get; set; } = TipoResidencia.Residente;
    public string? NombreComercial { get; set; }
    public string? PersonaContacto { get; set; }
    public string? CodigoINE { get; set; }
    public string? CNAE { get; set; }
    public string? LibroRegistro { get; set; }
    public string? RegistroMercantil { get; set; }
    public string? HojaRegistro { get; set; }
    public string? FolioRegistro { get; set; }
    public string? SeccionRegistro { get; set; }
    public string? TomoRegistro { get; set; }
    public string? DatosRegistroAdicionales { get; set; }
    
    // Datos financieros
    public string? CuentaBancaria { get; set; }
    public string? FormaPago { get; set; }
    public decimal? LimiteCredito { get; set; }
    public int? DiasPago { get; set; }
    public string? Contacto { get; set; }
    
    // Configuración de facturación - Facturas de Venta
    public string SerieFactura { get; set; } = "FAC";
    public int NumeroFacturaActual { get; set; } = 1;
    public int LongitudNumeroFactura { get; set; } = 4;
    public bool IncluirAñoEnSerie { get; set; } = true;
    
    // Configuración de facturación - Facturas de Compra
    public string SerieFacturaCompra { get; set; } = "FCOMP";
    public int NumeroFacturaCompraActual { get; set; } = 1;
    public int LongitudNumeroFacturaCompra { get; set; } = 4;
    public bool IncluirAñoEnSerieCompra { get; set; } = true;
    
    // Configuración de presupuestos
    public string SeriePresupuesto { get; set; } = "PRES";
    public int NumeroPresupuestoActual { get; set; } = 1;
    public int LongitudNumeroPresupuesto { get; set; } = 4;
    
    // Configuración general de facturación
    public decimal IVAPorDefecto { get; set; } = 21m;
    public int DiasVencimientoPorDefecto { get; set; } = 30;
    public decimal RetencionIRPFPorDefecto { get; set; } = 0m;
    
    // Certificado digital para firma electrónica
    public string? CertificadoRuta { get; set; }
    public string? CertificadoPassword { get; set; }
    
    // Multi-tenant: cada empresa tiene su propia configuración
    public int EmpresaId { get; set; }
    
    // Relación con Empresa (uno a uno)
    public Empresa Empresa { get; set; } = null!;
}
