using System.ComponentModel.DataAnnotations.Schema;

namespace FactioX.Models;

public enum EstadoTicketTpv
{
    Abierto,
    Aparcado,
    Cobrado,
    Anulado,
    Facturado
}

public enum TipoTicketTpv
{
    Venta,
    Devolucion
}

public class TicketTpv
{
    public int Id { get; set; }

    // Se asigna al cobrar para que la serie sea correlativa y sin huecos
    public string? Numero { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public DateTime? FechaCobro { get; set; }

    public int TerminalId { get; set; }
    public TpvTerminal? Terminal { get; set; }
    public int? SesionCajaId { get; set; }
    public TpvSesionCaja? SesionCaja { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public EstadoTicketTpv Estado { get; set; } = EstadoTicketTpv.Abierto;
    public TipoTicketTpv TipoTicket { get; set; } = TipoTicketTpv.Venta;

    public int? TicketOriginalId { get; set; }
    public TicketTpv? TicketOriginal { get; set; }
    public int? PresupuestoOrigenId { get; set; }
    public Presupuesto? PresupuestoOrigen { get; set; }
    // Factura completa (canje) o abono generado al devolver un ticket facturado
    public int? FacturaId { get; set; }
    public Factura? Factura { get; set; }

    public decimal DescuentoGlobal { get; set; }
    public decimal BaseImponible { get; set; }
    public decimal TotalIVA { get; set; }
    public decimal Total { get; set; }
    public decimal Entregado { get; set; }
    public decimal Cambio { get; set; }
    public string? Notas { get; set; }

    // Encadenamiento SHA-256 con el ticket anterior, preparado para Verifactu
    public string? Huella { get; set; }

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;

    public List<LineaDocumento> Lineas { get; set; } = new();
    public List<PagoTicketTpv> Pagos { get; set; } = new();

    [NotMapped]
    public bool EsDevolucion => TipoTicket == TipoTicketTpv.Devolucion;

    [NotMapped]
    public bool Editable => Estado is EstadoTicketTpv.Abierto or EstadoTicketTpv.Aparcado;

    public void CalcularTotales()
    {
        foreach (var linea in Lineas) linea.CalcularImportes();
        var desglose = DesgloseIVA();
        BaseImponible = desglose.Sum(d => d.Base);
        TotalIVA = desglose.Sum(d => d.Cuota);
        Total = BaseImponible + TotalIVA;
    }

    public List<(decimal Tipo, decimal Base, decimal Cuota)> DesgloseIVA()
    {
        var factor = 1 - DescuentoGlobal / 100m;
        return Lineas
            .GroupBy(l => l.IVA)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var baseIva = g.Sum(l => l.Cantidad * l.PrecioUnitario * (1 - l.Descuento / 100m)) * factor;
                return (g.Key,
                    Math.Round(baseIva, 2, MidpointRounding.AwayFromZero),
                    Math.Round(baseIva * g.Key / 100m, 2, MidpointRounding.AwayFromZero));
            })
            .ToList();
    }
}

public class PagoTicketTpv
{
    public int Id { get; set; }
    public int TicketTpvId { get; set; }
    public TicketTpv TicketTpv { get; set; } = null!;
    public int? FormaPagoPersonalizadaId { get; set; }
    public FormaPagoPersonalizada? FormaPagoPersonalizada { get; set; }
    public bool EsEfectivo { get; set; }
    public decimal Importe { get; set; }
    public string? Referencia { get; set; }
}
