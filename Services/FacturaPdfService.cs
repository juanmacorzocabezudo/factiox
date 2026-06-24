using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using FactioX.Models;

namespace FactioX.Services;

public interface IFacturaPdfService
{
    byte[] GenerarFacturaPdf(Factura factura, Cliente cliente, ConfiguracionEmpresa? config, bool paraEmail = true);
}

public class FacturaPdfService : IFacturaPdfService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public FacturaPdfService(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public byte[] GenerarFacturaPdf(Factura factura, Cliente cliente, ConfiguracionEmpresa? config, bool paraEmail = true)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // Tamaños según destino
        // Email/Web: tamaño normal para buena visualización en pantalla
        // Impresión: bien espaciado y legible
        var margen = paraEmail ? 50 : 40;
        var fuenteBase = paraEmail ? 11 : 10;
        var fuenteGrande = paraEmail ? 16 : 14;
        var fuenteMedia = paraEmail ? 13 : 12;
        var fuentePequena = paraEmail ? 10 : 9;
        var fuenteMuyPequena = paraEmail ? 9 : 8;
        var logoAncho = paraEmail ? 120 : 100;
        var logoAlto = paraEmail ? 60 : 50;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(margen);
                page.DefaultTextStyle(x => x.FontSize(fuenteBase));

                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        // Datos de la empresa (izquierda)
                        row.RelativeItem().Column(col =>
                        {
                            // Logo de la empresa si existe
                            if (!string.IsNullOrEmpty(config?.LogoRuta))
                            {
                                string logoPath = config.LogoRuta;
                                if (logoPath.StartsWith("/") || logoPath.StartsWith("\\"))
                                {
                                    logoPath = Path.Combine(_webHostEnvironment.WebRootPath, logoPath.TrimStart('/', '\\'));
                                }
                                
                                if (File.Exists(logoPath))
                                {
                                    col.Item().MaxWidth(logoAncho).MaxHeight(logoAlto).Image(logoPath);
                                    col.Item().PaddingTop(paraEmail ? 1 : 5);
                                }
                            }
                            
                            col.Item().Text(config?.NombreEmpresa ?? "FactioX")
                                .FontSize(fuenteMedia).Bold().FontColor("#003366");
                            col.Item().Text(config?.Direccion ?? "").FontSize(fuentePequena);
                            col.Item().Text($"{config?.CodigoPostal ?? ""} {config?.Ciudad ?? ""}").FontSize(fuentePequena);
                            col.Item().Text($"NIF: {config?.NIF ?? ""}").FontSize(fuentePequena);
                            col.Item().Text($"Tel: {config?.Telefono ?? ""}").FontSize(fuentePequena);
                            if (!string.IsNullOrEmpty(config?.Email))
                            {
                                col.Item().Text($"Email: {config.Email}").FontSize(fuentePequena);
                            }
                            if (!string.IsNullOrEmpty(config?.Web))
                            {
                                col.Item().Text($"Web: {config.Web}").FontSize(fuentePequena);
                            }
                        });

                        // Datos del cliente y factura (derecha)
                        row.RelativeItem().Column(col =>
                        {
                            // Encabezado FACTURA
                            col.Item().Background("#003366").Padding(2).Column(c =>
                            {
                                var tituloFactura = factura.TipoFactura == TipoFactura.Compra ? "FACTURA COMPRA" : "FACTURA VENTA";
                                c.Item().AlignCenter().Text(tituloFactura).FontSize(fuenteGrande).Bold().FontColor(Colors.White);
                                c.Item().AlignCenter().Text(factura.NumeroFactura).FontSize(fuenteBase).FontColor(Colors.White);
                            });
                            
                            col.Item().PaddingTop(paraEmail ? 1 : 5);
                            
                            // Cuadro de datos del cliente
                            col.Item().Background(Colors.Grey.Lighten3).Padding(paraEmail ? 2 : 5).Column(c =>
                            {
                                c.Item().Text("CLIENTE").Bold().FontSize(fuenteBase).FontColor("#003366");
                                
                                if (cliente.Tipo == TipoCliente.Particular)
                                {
                                    c.Item().Text($"{cliente.Nombre} {cliente.Apellidos}").FontSize(fuenteBase).Bold();
                                }
                                else
                                {
                                    c.Item().Text(cliente.NombreEmpresa ?? "").FontSize(fuenteBase).Bold();
                                    if (!string.IsNullOrEmpty(cliente.Nombre))
                                    {
                                        c.Item().Text($"Att: {cliente.Nombre}").FontSize(fuentePequena);
                                    }
                                }
                                
                                c.Item().Text($"NIF/CIF: {cliente.NIF}").FontSize(fuentePequena);
                                if (!string.IsNullOrEmpty(cliente.Direccion))
                                {
                                    c.Item().Text(cliente.Direccion).FontSize(fuentePequena);
                                }
                                if (!string.IsNullOrEmpty(cliente.CodigoPostal) || !string.IsNullOrEmpty(cliente.Ciudad))
                                {
                                    c.Item().Text($"{cliente.CodigoPostal} {cliente.Ciudad}").FontSize(fuentePequena);
                                }
                                if (!string.IsNullOrEmpty(cliente.Telefono))
                                {
                                    c.Item().Text($"Tel: {cliente.Telefono}").FontSize(fuentePequena);
                                }
                                if (!string.IsNullOrEmpty(cliente.Email))
                                {
                                    c.Item().Text($"Email: {cliente.Email}").FontSize(fuentePequena);
                                }
                            });
                        });
                    });

                    column.Item().PaddingTop(paraEmail ? 10 : 8);
                    
                    // Información de la factura
                    column.Item().Background(Colors.Grey.Lighten4).Padding(paraEmail ? 8 : 5).Row(row =>
                    {
                        row.AutoItem().Text($"Fecha Emisión: {factura.FechaEmision:dd/MM/yyyy}").FontSize(fuentePequena);
                        row.AutoItem().PaddingLeft(5).Text($"Vencimiento: {factura.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "N/A"}").FontSize(fuentePequena);
                        
                        // Mostrar forma de pago personalizada o enum
                        if (factura.FormaPagoPersonalizada != null)
                        {
                            row.AutoItem().PaddingLeft(5).Text($"Forma de Pago: {factura.FormaPagoPersonalizada.Nombre}").FontSize(fuentePequena);
                        }
                        else if (factura.FormaPago.HasValue)
                        {
                            row.AutoItem().PaddingLeft(5).Text($"Forma de Pago: {factura.FormaPago.Value}").FontSize(fuentePequena);
                        }
                    });

                    column.Item().PaddingVertical(paraEmail ? 5 : 4).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().Column(column =>
                {
                    // Espaciado inicial en el contenido
                    column.Item().PaddingTop(paraEmail ? 5 : 10);
                    
                    // Concepto si existe
                    if (!string.IsNullOrEmpty(factura.Concepto))
                    {
                        column.Item().Background("#fffbf0").BorderLeft(2).BorderColor("#FFC107")
                            .Padding(paraEmail ? 10 : 5).Text($"Concepto: {factura.Concepto}").FontSize(fuenteBase).Italic();
                        column.Item().PaddingVertical(paraEmail ? 8 : 8);
                    }

                    // Tabla de líneas
                    if (factura.Lineas != null && factura.Lineas.Any())
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                var headerPadding = paraEmail ? 6 : 4;
                                header.Cell().Background("#003366").Padding(headerPadding)
                                    .Text("Descripción").Bold().FontColor(Colors.White).FontSize(fuenteBase);
                                header.Cell().Background("#003366").Padding(headerPadding)
                                    .AlignCenter().Text("Cant.").Bold().FontColor(Colors.White).FontSize(fuenteBase);
                                header.Cell().Background("#003366").Padding(headerPadding)
                                    .AlignRight().Text("Precio Unit.").Bold().FontColor(Colors.White).FontSize(fuenteBase);
                                header.Cell().Background("#003366").Padding(headerPadding)
                                    .AlignRight().Text("IVA %").Bold().FontColor(Colors.White).FontSize(fuenteBase);
                                header.Cell().Background("#003366").Padding(headerPadding)
                                    .AlignRight().Text("Total").Bold().FontColor(Colors.White).FontSize(fuenteBase);
                            });

                            var cellPadding = paraEmail ? 4 : 3;
                            foreach (var linea in factura.Lineas)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(cellPadding)
                                    .Text(linea.Descripcion).FontSize(fuenteBase);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(cellPadding)
                                    .AlignCenter().Text(linea.Cantidad.ToString()).FontSize(fuenteBase);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(cellPadding)
                                    .AlignRight().Text(linea.PrecioUnitario.ToString("C2")).FontSize(fuenteBase);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(cellPadding)
                                    .AlignRight().Text($"{linea.IVA}%").FontSize(fuenteBase);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(cellPadding)
                                    .AlignRight().Text(linea.Total.ToString("C2")).Bold().FontSize(fuenteBase);
                            }
                        });

                        column.Item().PaddingTop(paraEmail ? 2 : 8);
                    }

                    // Totales
                    column.Item().AlignRight().Width(paraEmail ? 130 : 160).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        var totalPadding = paraEmail ? 1 : 2;
                        table.Cell().Padding(totalPadding).Text("Base Imponible:").Bold().FontSize(fuenteBase);
                        table.Cell().Padding(totalPadding).AlignRight().Text(factura.BaseImponible.ToString("C2")).FontSize(fuenteBase);

                        // Descuentos (si existen)
                        if (factura.DescuentosGenerales > 0)
                        {
                            table.Cell().Padding(totalPadding).Text("Descuentos:").FontSize(fuenteBase).FontColor(Colors.Green.Medium);
                            table.Cell().Padding(totalPadding).AlignRight().Text($"-{factura.DescuentosGenerales.ToString("C2")}").FontSize(fuenteBase).FontColor(Colors.Green.Medium);
                        }

                        // Recargos (si existen)
                        if (factura.RecargosGenerales > 0)
                        {
                            table.Cell().Padding(totalPadding).Text("Recargos:").FontSize(fuenteBase).FontColor(Colors.Orange.Medium);
                            table.Cell().Padding(totalPadding).AlignRight().Text($"+{factura.RecargosGenerales.ToString("C2")}").FontSize(fuenteBase).FontColor(Colors.Orange.Medium);
                        }

                        table.Cell().Padding(totalPadding).Text($"IVA ({factura.PorcentajeIVA}%):").Bold().FontSize(fuenteBase);
                        table.Cell().Padding(totalPadding).AlignRight().Text(factura.ImporteIVA.ToString("C2")).FontSize(fuenteBase);

                        // Retención (si existe)
                        if (factura.PorcentajeRetencion > 0)
                        {
                            table.Cell().Padding(totalPadding).Text($"Retención IRPF ({factura.PorcentajeRetencion}%):").Bold().FontColor(Colors.Red.Medium).FontSize(fuenteBase);
                            table.Cell().Padding(totalPadding).AlignRight().Text($"-{factura.ImporteRetencion.ToString("C2")}").FontSize(fuenteBase).FontColor(Colors.Red.Medium);
                        }

                        table.Cell().Background("#003366").Padding(totalPadding).Text("TOTAL:").Bold().FontSize(fuenteMedia).FontColor(Colors.White);
                        table.Cell().Background("#003366").Padding(totalPadding).AlignRight()
                            .Text(factura.Total.ToString("C2")).Bold().FontSize(fuenteMedia).FontColor(Colors.White);
                    });

                    // Observaciones si existen
                    if (!string.IsNullOrEmpty(factura.Observaciones))
                    {
                        column.Item().PaddingTop(paraEmail ? 2 : 8);
                        column.Item().Background(Colors.Grey.Lighten4).Padding(paraEmail ? 2 : 5).Column(col =>
                        {
                            col.Item().Text("Observaciones:").Bold().FontSize(fuenteBase).FontColor("#003366");
                            col.Item().Text(factura.Observaciones).FontSize(fuentePequena);
                        });
                    }
                    
                    // Pie de página
                    column.Item().PaddingTop(paraEmail ? 2 : 10).BorderTop(1).BorderColor(Colors.Grey.Medium)
                        .PaddingTop(paraEmail ? 1 : 5).AlignCenter().Column(col =>
                        {
                            col.Item().Text($"Gracias por confiar en {config?.NombreEmpresa ?? "FactioX"}")
                                .FontSize(fuenteMuyPequena).Italic().FontColor(Colors.Grey.Darken1);
                            if (!string.IsNullOrEmpty(config?.IBAN))
                            {
                                col.Item().Text($"Datos bancarios: IBAN {config.IBAN}")
                                    .FontSize(fuenteMuyPequena).FontColor(Colors.Grey.Darken1);
                            }
                        });
                });

                page.Footer().Column(footerColumn =>
                {
                    // Información legal y protección de datos
                    footerColumn.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1).PaddingTop(1)
                        .AlignCenter().Column(legalCol =>
                        {
                            legalCol.Item().Text("INFORMACIÓN LEGAL Y PROTECCIÓN DE DATOS")
                                .FontSize(fuenteMuyPequena).Bold().FontColor(Colors.Grey.Darken1);
                            
                            legalCol.Item().PaddingTop(0.5f).Text(txt =>
                            {
                                txt.Span($"Los datos personales facilitados serán tratados por {config?.NombreEmpresa ?? "FactioX"} con la finalidad de gestionar la relación comercial y contable. ")
                                    .FontSize(fuenteMuyPequena - 1).FontColor(Colors.Grey.Darken1);
                                txt.Span("La base legal del tratamiento es la ejecución del contrato y el cumplimiento de obligaciones legales. ")
                                    .FontSize(fuenteMuyPequena - 1).FontColor(Colors.Grey.Darken1);
                            });
                            
                            legalCol.Item().Text("Los datos no serán cedidos a terceros salvo obligación legal. Puede ejercer sus derechos de acceso, rectificación, supresión, limitación, portabilidad y oposición.")
                                .FontSize(fuenteMuyPequena - 1).FontColor(Colors.Grey.Darken1);
                            
                            legalCol.Item().PaddingTop(0.5f).Text(txt =>
                            {
                                txt.Span($"© {DateTime.Now.Year} {config?.NombreEmpresa ?? "FactioX"}. Todos los derechos reservados. ")
                                    .FontSize(fuenteMuyPequena - 1).FontColor(Colors.Grey.Darken1).Bold();
                                txt.Span("Este documento es propiedad del emisor y tiene efectos legales conforme a la normativa vigente.")
                                    .FontSize(fuenteMuyPequena - 1).FontColor(Colors.Grey.Darken1);
                            });
                        });
                    
                    // Número de página
                    footerColumn.Item().PaddingTop(1).AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                });
            });
        });

        return documento.GeneratePdf();
    }
}
