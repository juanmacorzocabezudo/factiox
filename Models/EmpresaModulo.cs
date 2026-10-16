namespace FactioX.Models;

public enum TipoModulo
{
    TPV,
    Contabilidad,
    Almacen,
    Produccion
}

public class EmpresaModulo
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public TipoModulo Modulo { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaActivacion { get; set; } = DateTime.Now;
    public DateTime? FechaDesactivacion { get; set; }
    public int? UsuarioActivacionId { get; set; }
}
