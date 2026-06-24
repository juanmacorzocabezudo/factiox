using System.Globalization;
using System.Text;
using System.Xml;
using FactioX.Data;
using FactioX.Models;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

// StringWriter que fuerza encoding UTF-8 para el XML
public class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}

public interface IFacturaEService
{
    Task<string> GenerarFacturaEXmlAsync(int facturaId);
}

public class FacturaEService : IFacturaEService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly ITenantService _tenantService;

    public FacturaEService(IDbContextFactory<ApplicationDbContext> dbFactory, ITenantService tenantService)
    {
        _dbFactory = dbFactory;
        _tenantService = tenantService;
    }

    public async Task<string> GenerarFacturaEXmlAsync(int facturaId)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        
        var empresaId = await _tenantService.GetEmpresaIdAsync();
        
        var factura = await context.Facturas
            .Include(f => f.Cliente)
            .Include(f => f.Lineas)
            .Include(f => f.FormaPagoPersonalizada)
            .FirstOrDefaultAsync(f => f.Id == facturaId && f.EmpresaId == empresaId);

        if (factura == null)
        {
            throw new Exception("Factura no encontrada");
        }

        var configuracion = await context.ConfiguracionEmpresa
            .FirstOrDefaultAsync(c => c.EmpresaId == factura.EmpresaId);

        if (configuracion == null)
        {
            throw new Exception("Configuración de empresa no encontrada");
        }

        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            Indent = false,
            OmitXmlDeclaration = false
        };

        using var stringWriter = new Utf8StringWriter();
        
        using (var writer = XmlWriter.Create(stringWriter, settings))
        {
            writer.WriteStartDocument();

            // Elemento raíz con namespaces (versión 3.2.1)
            writer.WriteStartElement("fe", "Facturae", "http://www.facturae.es/Facturae/2014/v3.2.1/Facturae");
            writer.WriteAttributeString("xmlns", "ds", null, "http://www.w3.org/2000/09/xmldsig#");

            // FileHeader
            EscribirFileHeader(writer, factura);

            // Parties (SellerParty y BuyerParty)
            EscribirParties(writer, factura, configuracion);

            // Invoices
            EscribirInvoice(writer, factura, configuracion);

            writer.WriteEndElement(); // Facturae
            writer.WriteEndDocument();
            writer.Flush();
        }

        return stringWriter.ToString();
    }

    private void EscribirFileHeader(XmlWriter writer, Factura factura)
    {
        writer.WriteStartElement("FileHeader");

        writer.WriteElementString("SchemaVersion", "3.2.1");
        writer.WriteElementString("Modality", "I"); // I = Individual
        writer.WriteElementString("InvoiceIssuerType", "EM"); // EM = Emisor

        writer.WriteStartElement("Batch");
        writer.WriteElementString("BatchIdentifier", $"{factura.NumeroFactura}-");
        writer.WriteElementString("InvoicesCount", "1");

        writer.WriteStartElement("TotalInvoicesAmount");
        writer.WriteElementString("TotalAmount", FormatDecimal(factura.Total));
        writer.WriteEndElement();

        writer.WriteStartElement("TotalOutstandingAmount");
        writer.WriteElementString("TotalAmount", FormatDecimal(factura.Total));
        writer.WriteEndElement();

        writer.WriteStartElement("TotalExecutableAmount");
        writer.WriteElementString("TotalAmount", FormatDecimal(factura.Total));
        writer.WriteEndElement();

        writer.WriteElementString("InvoiceCurrencyCode", "EUR");

        writer.WriteEndElement(); // Batch
        writer.WriteEndElement(); // FileHeader
    }

    private void EscribirParties(XmlWriter writer, Factura factura, ConfiguracionEmpresa config)
    {
        writer.WriteStartElement("Parties");

        // SellerParty (Empresa emisora)
        writer.WriteStartElement("SellerParty");

        writer.WriteStartElement("TaxIdentification");
        writer.WriteElementString("PersonTypeCode", config.TipoPersona == TipoPersona.Fisica ? "F" : "J");
        writer.WriteElementString("ResidenceTypeCode", config.TipoResidencia == TipoResidencia.Residente ? "R" : 
                                                       config.TipoResidencia == TipoResidencia.UnionEuropea ? "U" : "E");
        writer.WriteElementString("TaxIdentificationNumber", config.NIF);
        writer.WriteEndElement(); // TaxIdentification

        writer.WriteStartElement("LegalEntity");
        writer.WriteElementString("CorporateName", config.NombreEmpresa);
        
        if (!string.IsNullOrEmpty(config.NombreComercial))
            writer.WriteElementString("TradeName", config.NombreComercial);

        // RegistrationData (opcional)
        if (!string.IsNullOrEmpty(config.LibroRegistro))
        {
            writer.WriteStartElement("RegistrationData");
            if (!string.IsNullOrEmpty(config.LibroRegistro))
                writer.WriteElementString("Book", config.LibroRegistro);
            if (!string.IsNullOrEmpty(config.RegistroMercantil))
                writer.WriteElementString("RegisterOfCompaniesLocation", config.RegistroMercantil);
            if (!string.IsNullOrEmpty(config.HojaRegistro))
                writer.WriteElementString("Sheet", config.HojaRegistro);
            if (!string.IsNullOrEmpty(config.FolioRegistro))
                writer.WriteElementString("Folio", config.FolioRegistro);
            if (!string.IsNullOrEmpty(config.SeccionRegistro))
                writer.WriteElementString("Section", config.SeccionRegistro);
            if (!string.IsNullOrEmpty(config.TomoRegistro))
                writer.WriteElementString("Volume", config.TomoRegistro);
            if (!string.IsNullOrEmpty(config.DatosRegistroAdicionales))
                writer.WriteElementString("AdditionalRegistrationData", config.DatosRegistroAdicionales);
            writer.WriteEndElement(); // RegistrationData
        }

        // AddressInSpain
        writer.WriteStartElement("AddressInSpain");
        writer.WriteElementString("Address", config.Direccion);
        writer.WriteElementString("PostCode", config.CodigoPostal);
        writer.WriteElementString("Town", config.Ciudad);
        writer.WriteElementString("Province", config.Provincia);
        writer.WriteElementString("CountryCode", config.Pais);
        writer.WriteEndElement(); // AddressInSpain

        // ContactDetails
        writer.WriteStartElement("ContactDetails");
        writer.WriteElementString("Telephone", config.Telefono);
        if (!string.IsNullOrEmpty(config.Fax))
            writer.WriteElementString("TeleFax", config.Fax);
        if (!string.IsNullOrEmpty(config.Web))
            writer.WriteElementString("WebAddress", config.Web);
        writer.WriteElementString("ElectronicMail", config.Email);
        if (!string.IsNullOrEmpty(config.PersonaContacto))
            writer.WriteElementString("ContactPersons", config.PersonaContacto);
        if (!string.IsNullOrEmpty(config.CNAE))
            writer.WriteElementString("CnoCnae", config.CNAE);
        if (!string.IsNullOrEmpty(config.CodigoINE))
            writer.WriteElementString("INETownCode", config.CodigoINE);
        writer.WriteEndElement(); // ContactDetails

        writer.WriteEndElement(); // LegalEntity
        writer.WriteEndElement(); // SellerParty

        // BuyerParty (Cliente)
        if (factura.Cliente != null)
        {
            writer.WriteStartElement("BuyerParty");

            writer.WriteStartElement("TaxIdentification");
            writer.WriteElementString("PersonTypeCode", factura.Cliente.TipoPersona == TipoPersona.Fisica ? "F" : "J");
            writer.WriteElementString("ResidenceTypeCode", 
                factura.Cliente.TipoResidencia == TipoResidencia.Residente ? "R" :
                factura.Cliente.TipoResidencia == TipoResidencia.UnionEuropea ? "U" : "E");
            writer.WriteElementString("TaxIdentificationNumber", factura.Cliente.NIF);
            writer.WriteEndElement(); // TaxIdentification

            if (factura.Cliente.TipoPersona == TipoPersona.Juridica)
            {
                writer.WriteStartElement("LegalEntity");
                writer.WriteElementString("CorporateName", factura.Cliente.NombreEmpresa ?? factura.Cliente.Nombre);
            }
            else
            {
                writer.WriteStartElement("Individual");
                writer.WriteElementString("Name", factura.Cliente.Nombre ?? "");
                writer.WriteElementString("FirstSurname", factura.Cliente.Apellidos ?? "");
                writer.WriteElementString("SecondSurname", "");
            }

            // AddressInSpain
            writer.WriteStartElement("AddressInSpain");
            writer.WriteElementString("Address", factura.Cliente.Direccion ?? "");
            writer.WriteElementString("PostCode", factura.Cliente.CodigoPostal ?? "");
            writer.WriteElementString("Town", factura.Cliente.Ciudad ?? "");
            writer.WriteElementString("Province", factura.Cliente.Provincia ?? "");
            writer.WriteElementString("CountryCode", factura.Cliente.Pais);
            writer.WriteEndElement(); // AddressInSpain

            // ContactDetails
            writer.WriteStartElement("ContactDetails");
            writer.WriteElementString("Telephone", factura.Cliente.Telefono);
            if (!string.IsNullOrEmpty(factura.Cliente.Fax))
                writer.WriteElementString("TeleFax", factura.Cliente.Fax);
            if (!string.IsNullOrEmpty(factura.Cliente.PersonaContacto))
                writer.WriteElementString("ContactPersons", factura.Cliente.PersonaContacto);
            if (!string.IsNullOrEmpty(factura.Cliente.CodigoINE))
                writer.WriteElementString("INETownCode", factura.Cliente.CodigoINE);
            writer.WriteEndElement(); // ContactDetails

            writer.WriteEndElement(); // LegalEntity o Individual
            writer.WriteEndElement(); // BuyerParty
        }

        writer.WriteEndElement(); // Parties
    }

    private void EscribirInvoice(XmlWriter writer, Factura factura, ConfiguracionEmpresa config)
    {
        writer.WriteStartElement("Invoices");
        writer.WriteStartElement("Invoice");

        // InvoiceHeader
        writer.WriteStartElement("InvoiceHeader");
        
        var partesNumero = ExtraerNumeroYSerie(factura.NumeroFactura);
        writer.WriteElementString("InvoiceNumber", partesNumero.numero);
        writer.WriteElementString("InvoiceSeriesCode", partesNumero.serie);
        writer.WriteElementString("InvoiceDocumentType", "FC"); // Factura Completa
        writer.WriteElementString("InvoiceClass", 
            factura.ClaseFactura == ClaseFactura.Original ? "OO" :
            factura.ClaseFactura == ClaseFactura.Rectificativa ? "OR" : "OC");
        
        writer.WriteEndElement(); // InvoiceHeader

        // InvoiceIssueData
        writer.WriteStartElement("InvoiceIssueData");
        writer.WriteElementString("IssueDate", factura.FechaEmision.ToString("yyyy-MM-dd"));
        
        if (factura.FechaOperacion.HasValue)
            writer.WriteElementString("OperationDate", factura.FechaOperacion.Value.ToString("yyyy-MM-dd"));

        if (!string.IsNullOrEmpty(factura.CodigoPostalExpedicion) || !string.IsNullOrEmpty(factura.LugarExpedicion))
        {
            writer.WriteStartElement("PlaceOfIssue");
            writer.WriteElementString("PostCode", factura.CodigoPostalExpedicion ?? config.CodigoPostal);
            writer.WriteElementString("PlaceOfIssueDescription", factura.LugarExpedicion ?? config.Ciudad);
            writer.WriteEndElement(); // PlaceOfIssue
        }

        if (factura.FechaPeriodoInicio.HasValue && factura.FechaPeriodoFin.HasValue)
        {
            writer.WriteStartElement("InvoicingPeriod");
            writer.WriteElementString("StartDate", factura.FechaPeriodoInicio.Value.ToString("yyyy-MM-dd"));
            writer.WriteElementString("EndDate", factura.FechaPeriodoFin.Value.ToString("yyyy-MM-dd"));
            writer.WriteEndElement(); // InvoicingPeriod
        }

        writer.WriteElementString("InvoiceCurrencyCode", "EUR");
        writer.WriteElementString("TaxCurrencyCode", "EUR");
        writer.WriteElementString("LanguageName", "es");
        
        writer.WriteEndElement(); // InvoiceIssueData

        // TaxesOutputs (IVA)
        writer.WriteStartElement("TaxesOutputs");
        writer.WriteStartElement("Tax");
        writer.WriteElementString("TaxTypeCode", "01"); // 01 = IVA
        writer.WriteElementString("TaxRate", FormatTaxRate(factura.PorcentajeIVA));
        writer.WriteStartElement("TaxableBase");
        writer.WriteElementString("TotalAmount", FormatDecimal(factura.BaseImponible));
        writer.WriteEndElement();
        writer.WriteStartElement("TaxAmount");
        writer.WriteElementString("TotalAmount", FormatDecimal(factura.ImporteIVA));
        writer.WriteEndElement();
        writer.WriteEndElement(); // Tax
        writer.WriteEndElement(); // TaxesOutputs

        // TaxesWithheld (Retención si existe)
        if (factura.PorcentajeRetencion > 0)
        {
            writer.WriteStartElement("TaxesWithheld");
            writer.WriteStartElement("Tax");
            writer.WriteElementString("TaxTypeCode", "04"); // 04 = IRPF
            writer.WriteElementString("TaxRate", FormatTaxRate(factura.PorcentajeRetencion));
            writer.WriteStartElement("TaxableBase");
            writer.WriteElementString("TotalAmount", FormatDecimal(factura.BaseImponible));
            writer.WriteEndElement();
            writer.WriteStartElement("TaxAmount");
            writer.WriteElementString("TotalAmount", FormatDecimal(factura.ImporteRetencion));
            writer.WriteEndElement();
            writer.WriteEndElement(); // Tax
            writer.WriteEndElement(); // TaxesWithheld
        }

        // InvoiceTotals
        writer.WriteStartElement("InvoiceTotals");
        writer.WriteElementString("TotalGrossAmount", FormatDecimal(factura.BaseImponible));
        writer.WriteElementString("TotalGeneralDiscounts", FormatDecimal(factura.DescuentosGenerales));
        writer.WriteElementString("TotalGeneralSurcharges", FormatDecimal(factura.RecargosGenerales));
        
        var totalAntesImpuestos = factura.BaseImponible - factura.DescuentosGenerales + factura.RecargosGenerales;
        writer.WriteElementString("TotalGrossAmountBeforeTaxes", FormatDecimal(totalAntesImpuestos));
        writer.WriteElementString("TotalTaxOutputs", FormatDecimal(factura.ImporteIVA));
        writer.WriteElementString("TotalTaxesWithheld", FormatDecimal(factura.ImporteRetencion));
        writer.WriteElementString("InvoiceTotal", FormatDecimal(factura.Total));
        writer.WriteElementString("TotalOutstandingAmount", FormatDecimal(factura.Total));
        writer.WriteElementString("TotalExecutableAmount", FormatDecimal(factura.Total));
        
        writer.WriteEndElement(); // InvoiceTotals

        // Items (Líneas de factura)
        writer.WriteStartElement("Items");
        
        if (factura.Lineas != null && factura.Lineas.Any())
        {
            foreach (var linea in factura.Lineas)
            {
                writer.WriteStartElement("InvoiceLine");
                
                writer.WriteElementString("ItemDescription", linea.Descripcion);
                writer.WriteElementString("Quantity", linea.Cantidad.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("UnitOfMeasure", "01"); // 01 = Unidades
                writer.WriteElementString("UnitPriceWithoutTax", FormatDecimal(linea.PrecioUnitario));
                writer.WriteElementString("TotalCost", FormatDecimal(linea.Subtotal));
                writer.WriteElementString("GrossAmount", FormatDecimal(linea.Subtotal));
                
                // TaxesOutputs por línea
                writer.WriteStartElement("TaxesOutputs");
                writer.WriteStartElement("Tax");
                writer.WriteElementString("TaxTypeCode", "01");
                writer.WriteElementString("TaxRate", FormatTaxRate(linea.IVA));
                writer.WriteStartElement("TaxableBase");
                writer.WriteElementString("TotalAmount", FormatDecimal(linea.Subtotal));
                writer.WriteEndElement();
                writer.WriteStartElement("TaxAmount");
                writer.WriteElementString("TotalAmount", FormatDecimal(linea.ImporteIVA));
                writer.WriteEndElement();
                writer.WriteEndElement(); // Tax
                writer.WriteEndElement(); // TaxesOutputs
                
                writer.WriteEndElement(); // InvoiceLine
            }
        }
        else
        {
            // Línea genérica si no hay líneas detalladas
            writer.WriteStartElement("InvoiceLine");
            
            writer.WriteElementString("ItemDescription", factura.Concepto ?? "Servicio prestado");
            writer.WriteElementString("Quantity", "1");
            writer.WriteElementString("UnitOfMeasure", "01");
            writer.WriteElementString("UnitPriceWithoutTax", FormatDecimal(factura.BaseImponible));
            writer.WriteElementString("TotalCost", FormatDecimal(factura.BaseImponible));
            writer.WriteElementString("GrossAmount", FormatDecimal(factura.BaseImponible));
            
            writer.WriteStartElement("TaxesOutputs");
            writer.WriteStartElement("Tax");
            writer.WriteElementString("TaxTypeCode", "01");
            writer.WriteElementString("TaxRate", FormatTaxRate(factura.PorcentajeIVA));
            writer.WriteStartElement("TaxableBase");
            writer.WriteElementString("TotalAmount", FormatDecimal(factura.BaseImponible));
            writer.WriteEndElement();
            writer.WriteStartElement("TaxAmount");
            writer.WriteElementString("TotalAmount", FormatDecimal(factura.ImporteIVA));
            writer.WriteEndElement();
            writer.WriteEndElement(); // Tax
            writer.WriteEndElement(); // TaxesOutputs
            
            writer.WriteEndElement(); // InvoiceLine
        }
        
        writer.WriteEndElement(); // Items

        // PaymentDetails (Forma de pago)
        if (factura.FormaPagoPersonalizada != null || factura.FormaPago.HasValue)
        {
            writer.WriteStartElement("PaymentDetails");
            writer.WriteStartElement("Installment");
            
            if (factura.FechaVencimiento.HasValue)
                writer.WriteElementString("InstallmentDueDate", factura.FechaVencimiento.Value.ToString("yyyy-MM-dd"));
            
            writer.WriteElementString("InstallmentAmount", FormatDecimal(factura.Total));
            
            // Determinar código de forma de pago
            string codigoFormaPago = "01"; // Por defecto: Contado
            if (factura.FormaPago.HasValue)
            {
                codigoFormaPago = ConvertirFormaPago(factura.FormaPago.Value);
            }
            writer.WriteElementString("PaymentMeans", codigoFormaPago);
            
            if (!string.IsNullOrEmpty(config.IBAN))
            {
                writer.WriteStartElement("AccountToBeCredited");
                writer.WriteElementString("IBAN", config.IBAN);
                writer.WriteEndElement();
            }
            
            writer.WriteEndElement(); // Installment
            writer.WriteEndElement(); // PaymentDetails
        }

        writer.WriteEndElement(); // Invoice
        writer.WriteEndElement(); // Invoices
    }

    private string FormatDecimal(decimal value)
    {
        return value.ToString("0.0##", CultureInfo.InvariantCulture);
    }

    private string FormatTaxRate(decimal value)
    {
        return value % 1 == 0 
            ? value.ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("0.0##", CultureInfo.InvariantCulture);
    }

    private (string serie, string numero) ExtraerNumeroYSerie(string numeroFactura)
    {
        var partes = numeroFactura.Split('-');
        if (partes.Length >= 2)
        {
            var serie = string.Join("-", partes.Take(partes.Length - 1));
            var numero = partes.Last();
            return (serie, numero);
        }
        return ("", numeroFactura);
    }

    private string ConvertirFormaPago(FormaPago formaPago)
    {
        return formaPago switch
        {
            FormaPago.Transferencia => "04",
            FormaPago.Domiciliacion => "02",
            FormaPago.TarjetaCredito => "48",
            FormaPago.TarjetaDebito => "49",
            _ => "01" // Efectivo, Bizum, etc.
        };
    }
}
