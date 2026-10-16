using FactioX.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FactioX.Services;

public interface ITicketPdfService
{
    byte[] GenerarTicketPdf(TicketTpv ticket, ConfiguracionEmpresa config, int anchoMm);
    byte[] GenerarInformeZPdf(InformeCajaTpv informe, ConfiguracionEmpresa config, int anchoMm);
}

public class TicketPdfService : ITicketPdfService
{
    public byte[] GenerarTicketPdf(TicketTpv ticket, ConfiguracionEmpresa config, int anchoMm)
    {
        var titulo = ticket.EsDevolucion ? "FACTURA SIMPLIFICADA RECTIFICATIVA" : "FACTURA SIMPLIFICADA";
        return Generar(anchoMm, col =>
        {
            Cabecera(col, config);
            col.Item().PaddingTop(4).AlignCenter().Text(titulo).Bold();
            col.Item().Text($"Nº: {ticket.Numero}");
            col.Item().Text($"Fecha: {(ticket.FechaCobro ?? ticket.Fecha):dd/MM/yyyy HH:mm}");
            if (ticket.Terminal != null) col.Item().Text($"Terminal: {ticket.Terminal.Codigo}");
            if (ticket.Usuario != null) col.Item().Text($"Atendido por: {ticket.Usuario.NombreCompleto}");
            if (ticket.EsDevolucion && ticket.TicketOriginal != null)
                col.Item().Text($"Rectifica a: {ticket.TicketOriginal.Numero}");
            if (ticket.Cliente != null)
            {
                var nombre = ticket.Cliente.Tipo == TipoCliente.Empresa
                    ? ticket.Cliente.NombreEmpresa
                    : $"{ticket.Cliente.Nombre} {ticket.Cliente.Apellidos}".Trim();
                col.Item().Text($"Cliente: {nombre} ({ticket.Cliente.NIF})");
            }

            Separador(col);
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(4);
                    c.RelativeColumn(1.4f);
                    c.RelativeColumn(2);
                });
                table.Header(h =>
                {
                    h.Cell().Text("Artículo").Bold();
                    h.Cell().AlignRight().Text("Ud").Bold();
                    h.Cell().AlignRight().Text("Importe").Bold();
                });
                foreach (var linea in ticket.Lineas)
                {
                    var pvp = linea.PrecioUnitario * (1 + linea.IVA / 100m);
                    var importe = linea.Cantidad * pvp * (1 - linea.Descuento / 100m);
                    table.Cell().Column(c =>
                    {
                        c.Item().Text(linea.Descripcion);
                        var detalle = $"{pvp:N2} €";
                        if (linea.Descuento != 0) detalle += $"  -{linea.Descuento:0.##}%";
                        c.Item().Text(detalle).FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                    });
                    table.Cell().AlignRight().Text($"{linea.Cantidad:0.##}");
                    table.Cell().AlignRight().Text($"{importe:N2}");
                }
            });

            Separador(col);
            if (ticket.DescuentoGlobal != 0)
                Fila(col, $"Descuento global {ticket.DescuentoGlobal:0.##}%", "");
            col.Item().Row(r =>
            {
                r.RelativeItem().Text("TOTAL").Bold().FontSize(11);
                r.RelativeItem().AlignRight().Text($"{ticket.Total:N2} €").Bold().FontSize(11);
            });

            col.Item().PaddingTop(3).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.RelativeColumn();
                    c.RelativeColumn();
                });
                table.Header(h =>
                {
                    h.Cell().Text("IVA").Bold();
                    h.Cell().AlignRight().Text("Base").Bold();
                    h.Cell().AlignRight().Text("Cuota").Bold();
                });
                foreach (var d in ticket.DesgloseIVA())
                {
                    table.Cell().Text($"{d.Tipo:0.##}%");
                    table.Cell().AlignRight().Text($"{d.Base:N2}");
                    table.Cell().AlignRight().Text($"{d.Cuota:N2}");
                }
            });

            Separador(col);
            foreach (var pago in ticket.Pagos.GroupBy(p => p.FormaPagoPersonalizada?.Nombre ?? (p.EsEfectivo ? "Efectivo" : "Pago")))
                Fila(col, pago.Key, $"{pago.Sum(p => p.Importe):N2} €");
            if (!ticket.EsDevolucion && ticket.Cambio > 0)
            {
                Fila(col, "Entregado", $"{ticket.Entregado:N2} €");
                Fila(col, "Cambio", $"{ticket.Cambio:N2} €");
            }

            if (ticket.Factura != null)
                col.Item().PaddingTop(3).Text($"{(ticket.EsDevolucion ? "Abono" : "Factura")}: {ticket.Factura.NumeroFactura}");
            if (!string.IsNullOrWhiteSpace(config.TpvTextoPieTicket))
                col.Item().PaddingTop(6).AlignCenter().Text(config.TpvTextoPieTicket);
            if (!string.IsNullOrEmpty(ticket.Huella))
                col.Item().PaddingTop(4).AlignCenter().Text($"Huella: {ticket.Huella[..16]}").FontSize(6).FontColor(Colors.Grey.Darken1);
        });
    }

    public byte[] GenerarInformeZPdf(InformeCajaTpv informe, ConfiguracionEmpresa config, int anchoMm)
    {
        var sesion = informe.Sesion;
        return Generar(anchoMm, col =>
        {
            Cabecera(col, config);
            col.Item().PaddingTop(4).AlignCenter().Text(sesion.Estado == EstadoSesionCaja.Cerrada ? "INFORME Z" : "INFORME X (PARCIAL)").Bold();
            col.Item().Text($"Terminal: {informe.Terminal}");
            col.Item().Text($"Apertura: {sesion.FechaApertura:dd/MM/yyyy HH:mm}");
            if (sesion.FechaCierre.HasValue) col.Item().Text($"Cierre: {sesion.FechaCierre:dd/MM/yyyy HH:mm}");

            Separador(col);
            Fila(col, $"Ventas ({informe.NumeroTickets})", $"{informe.TotalVentas:N2} €");
            Fila(col, $"Devoluciones ({informe.NumeroDevoluciones})", $"{informe.TotalDevoluciones:N2} €");
            Fila(col, "TOTAL NETO", $"{informe.TotalNeto:N2} €", negrita: true);

            Separador(col);
            col.Item().Text("Por forma de pago").Bold();
            foreach (var (forma, importe) in informe.PorFormaPago)
                Fila(col, forma, $"{importe:N2} €");

            Separador(col);
            col.Item().Text("Por tipo de IVA").Bold();
            foreach (var (tipo, baseIva, cuota) in informe.PorIVA)
                Fila(col, $"{tipo:0.##}%  Base {baseIva:N2}", $"{cuota:N2} €");

            Separador(col);
            col.Item().Text("Efectivo").Bold();
            Fila(col, "Fondo inicial", $"{sesion.FondoInicial:N2} €");
            Fila(col, "Cobros en efectivo", $"{informe.CobrosEfectivo:N2} €");
            Fila(col, "Entradas", $"{informe.Entradas:N2} €");
            Fila(col, "Salidas", $"-{informe.Salidas:N2} €");
            Fila(col, "Esperado", $"{informe.EfectivoEsperado:N2} €", negrita: true);
            if (informe.EfectivoContado.HasValue)
            {
                Fila(col, "Contado", $"{informe.EfectivoContado:N2} €");
                Fila(col, "Descuadre", $"{informe.Descuadre:N2} €", negrita: true);
            }
            col.Item().PaddingTop(6).AlignCenter().Text($"Emitido {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(6.5f);
        });
    }

    private static byte[] Generar(int anchoMm, Action<ColumnDescriptor> contenido)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.ContinuousSize(anchoMm, Unit.Millimetre);
                page.Margin(3, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(anchoMm <= 58 ? 7 : 8));
                page.Content().Column(contenido);
            });
        }).GeneratePdf();
    }

    private static void Cabecera(ColumnDescriptor col, ConfiguracionEmpresa config)
    {
        col.Item().AlignCenter().Text(config.NombreComercial ?? config.NombreEmpresa).Bold().FontSize(10);
        if (!string.IsNullOrWhiteSpace(config.NombreComercial) && config.NombreComercial != config.NombreEmpresa)
            col.Item().AlignCenter().Text(config.NombreEmpresa);
        col.Item().AlignCenter().Text($"NIF: {config.NIF}");
        col.Item().AlignCenter().Text(config.Direccion);
        col.Item().AlignCenter().Text($"{config.CodigoPostal} {config.Ciudad}");
        if (!string.IsNullOrWhiteSpace(config.Telefono))
            col.Item().AlignCenter().Text($"Tel: {config.Telefono}");
    }

    private static void Separador(ColumnDescriptor col) =>
        col.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);

    private static void Fila(ColumnDescriptor col, string izquierda, string derecha, bool negrita = false)
    {
        col.Item().Row(r =>
        {
            var izq = r.RelativeItem().Text(izquierda);
            var der = r.RelativeItem().AlignRight().Text(derecha);
            if (negrita)
            {
                izq.Bold();
                der.Bold();
            }
        });
    }
}
