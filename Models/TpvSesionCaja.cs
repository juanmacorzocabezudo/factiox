namespace FactioX.Models;

public enum EstadoSesionCaja
{
    Abierta,
    Cerrada
}

public enum TipoMovimientoCaja
{
    Entrada,
    Salida
}

public class TpvSesionCaja
{
    public int Id { get; set; }
    public int TerminalId { get; set; }
    public TpvTerminal Terminal { get; set; } = null!;
    public int UsuarioAperturaId { get; set; }
    public DateTime FechaApertura { get; set; } = DateTime.Now;
    public decimal FondoInicial { get; set; }
    public int? UsuarioCierreId { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal? EfectivoContado { get; set; }
    public decimal EfectivoEsperado { get; set; }
    public decimal Descuadre { get; set; }
    public EstadoSesionCaja Estado { get; set; } = EstadoSesionCaja.Abierta;
    public string? Observaciones { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public List<TpvMovimientoCaja> Movimientos { get; set; } = new();
}

public class TpvMovimientoCaja
{
    public int Id { get; set; }
    public int SesionCajaId { get; set; }
    public TpvSesionCaja SesionCaja { get; set; } = null!;
    public TipoMovimientoCaja Tipo { get; set; }
    public decimal Importe { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
}
