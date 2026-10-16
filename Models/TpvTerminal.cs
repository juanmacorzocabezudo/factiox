using System.ComponentModel.DataAnnotations;

namespace FactioX.Models;

public class TpvTerminal
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código es obligatorio")]
    [MaxLength(20)]
    public string Codigo { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    [MaxLength(10)]
    public string? SerieTicket { get; set; }

    public int NumeroTicketActual { get; set; }

    public int? ClienteGenericoId { get; set; }
    public Cliente? ClienteGenerico { get; set; }

    public int? FormaPagoPorDefectoId { get; set; }
    public FormaPagoPersonalizada? FormaPagoPorDefecto { get; set; }

    [Range(58, 80)]
    public int ImpresoraAnchoMm { get; set; } = 80;

    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
}
