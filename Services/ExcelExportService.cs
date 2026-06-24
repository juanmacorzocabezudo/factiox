using FactioX.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace FactioX.Services;

public interface IExcelExportService
{
    byte[] ExportarFacturasAExcel(List<Factura> facturas, string tipoFactura);
}

public class ExcelExportService : IExcelExportService
{
    public byte[] ExportarFacturasAExcel(List<Factura> facturas, string tipoFactura)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add($"Facturas {tipoFactura}");
        
        // Encabezados
        var headers = new[]
        {
            "Número Factura", "Fecha Emisión", "Fecha Vencimiento", 
            tipoFactura == "Venta" ? "Cliente/Empresa" : "Proveedor",
            "Persona Contacto", "NIF/CIF", "Dirección", "Código Postal", 
            "Ciudad", "Provincia", "País", "Teléfono", "Email",
            "Base Imponible", "% IVA", "Importe IVA", 
            "Cargos", "% Retención", "Importe Retención", "Total", 
            "Empresa", "Concepto", "Observaciones"
        };
        
        // Aplicar estilo a los encabezados
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }
        
        // Datos
        int row = 2;
        foreach (var factura in facturas)
        {
            var tieneCliente = tipoFactura == "Venta";
            
            string nombreContacto = "";
            string personaContacto = "";
            string nif = "";
            string direccion = "";
            string codigoPostal = "";
            string ciudad = "";
            string provincia = "";
            string pais = "";
            string telefono = "";
            string email = "";
            
            if (tieneCliente && factura.Cliente != null)
            {
                if (factura.Cliente.Tipo == TipoCliente.Empresa)
                {
                    nombreContacto = factura.Cliente.NombreEmpresa ?? "";
                    personaContacto = factura.Cliente.Nombre ?? "";
                }
                else
                {
                    nombreContacto = $"{factura.Cliente.Nombre} {factura.Cliente.Apellidos}".Trim();
                    personaContacto = "";
                }
                nif = factura.Cliente.NIF ?? "";
                direccion = factura.Cliente.Direccion ?? "";
                codigoPostal = factura.Cliente.CodigoPostal ?? "";
                ciudad = factura.Cliente.Ciudad ?? "";
                provincia = factura.Cliente.Provincia ?? "";
                pais = factura.Cliente.Pais ?? "";
                telefono = factura.Cliente.Telefono ?? "";
                email = factura.Cliente.Email ?? "";
            }
            else if (!tieneCliente && factura.Proveedor != null)
            {
                nombreContacto = factura.Proveedor.Nombre ?? "";
                personaContacto = factura.Proveedor.PersonaContacto ?? "";
                nif = factura.Proveedor.NIF ?? "";
                direccion = factura.Proveedor.Direccion ?? "";
                codigoPostal = factura.Proveedor.CodigoPostal ?? "";
                ciudad = factura.Proveedor.Ciudad ?? "";
                provincia = factura.Proveedor.Provincia ?? "";
                pais = factura.Proveedor.Pais ?? "";
                telefono = factura.Proveedor.Telefono ?? "";
                email = factura.Proveedor.Email ?? "";
            }
            
            int col = 1;
            worksheet.Cells[row, col++].Value = factura.NumeroFactura;
            worksheet.Cells[row, col++].Value = factura.FechaEmision.ToString("dd/MM/yyyy");
            worksheet.Cells[row, col++].Value = factura.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "";
            worksheet.Cells[row, col++].Value = nombreContacto;
            worksheet.Cells[row, col++].Value = personaContacto;
            worksheet.Cells[row, col++].Value = nif;
            worksheet.Cells[row, col++].Value = direccion;
            worksheet.Cells[row, col++].Value = codigoPostal;
            worksheet.Cells[row, col++].Value = ciudad;
            worksheet.Cells[row, col++].Value = provincia;
            worksheet.Cells[row, col++].Value = pais;
            worksheet.Cells[row, col++].Value = telefono;
            worksheet.Cells[row, col++].Value = email;
            
            worksheet.Cells[row, col].Value = factura.BaseImponible;
            worksheet.Cells[row, col++].Style.Numberformat.Format = "#,##0.00 €";
            
            worksheet.Cells[row, col].Value = factura.PorcentajeIVA;
            worksheet.Cells[row, col++].Style.Numberformat.Format = "0.00%";
            
            worksheet.Cells[row, col].Value = factura.ImporteIVA;
            worksheet.Cells[row, col++].Style.Numberformat.Format = "#,##0.00 €";
            
            worksheet.Cells[row, col].Value = factura.CargosAdicionales;
            worksheet.Cells[row, col++].Style.Numberformat.Format = "#,##0.00 €";
            
            worksheet.Cells[row, col].Value = factura.PorcentajeRetencion;
            worksheet.Cells[row, col++].Style.Numberformat.Format = "0.00%";
            
            worksheet.Cells[row, col].Value = factura.ImporteRetencion;
            worksheet.Cells[row, col++].Style.Numberformat.Format = "#,##0.00 €";
            
            worksheet.Cells[row, col].Value = factura.Total;
            worksheet.Cells[row, col++].Style.Numberformat.Format = "#,##0.00 €";
            
            worksheet.Cells[row, col++].Value = factura.Empresa?.NombreComercial ?? "";
            worksheet.Cells[row, col++].Value = factura.Concepto ?? "";
            worksheet.Cells[row, col++].Value = factura.Observaciones ?? "";
            
            // Aplicar bordes
            for (int i = 1; i <= headers.Length; i++)
            {
                worksheet.Cells[row, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            
            row++;
        }
        
        // Ajustar ancho de columnas
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        
        // Congelar la primera fila
        worksheet.View.FreezePanes(2, 1);
        
        // Agregar filtros
        worksheet.Cells[1, 1, 1, headers.Length].AutoFilter = true;
        
        return package.GetAsByteArray();
    }
}
