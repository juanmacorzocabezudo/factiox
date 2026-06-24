using System.Text;
using System.Xml;
using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;

namespace FactioX.Services;

public interface ISEPAService
{
    Task<string> GenerarXMLSEPA(List<int> pagoIds, int empresaId);
}

public class SEPAService : ISEPAService
{
    private readonly ApplicationDbContext _context;

    public SEPAService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerarXMLSEPA(List<int> pagoIds, int empresaId)
    {
        // Obtener configuración de la empresa
        var configuracion = await _context.ConfiguracionEmpresa
            .Include(c => c.Empresa)
            .FirstOrDefaultAsync(c => c.EmpresaId == empresaId);

        if (configuracion == null)
            throw new Exception("No se encontró la configuración de la empresa");

        if (string.IsNullOrEmpty(configuracion.IBAN))
            throw new Exception("La empresa no tiene IBAN configurado");

        // Obtener los pagos con sus facturas y clientes
        var pagos = await _context.PagosFactura
            .Include(p => p.Factura)
                .ThenInclude(f => f.Cliente)
            .Where(p => pagoIds.Contains(p.Id))
            .ToListAsync();

        if (!pagos.Any())
            throw new Exception("No se encontraron pagos para exportar");

        // Validar que todos los clientes tengan IBAN
        var clientesSinIBAN = pagos
            .Where(p => string.IsNullOrEmpty(p.Factura.Cliente?.CuentaBancaria))
            .Select(p => p.Factura.Cliente?.Nombre ?? "Desconocido")
            .Distinct()
            .ToList();

        if (clientesSinIBAN.Any())
            throw new Exception($"Los siguientes clientes no tienen IBAN: {string.Join(", ", clientesSinIBAN)}");

        // Generar XML SEPA
        var xml = GenerarXMLContent(configuracion, pagos);
        return xml;
    }

    private string GenerarXMLContent(ConfiguracionEmpresa configuracion, List<PagoFactura> pagos)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false
        };

        using var stream = new MemoryStream();
        using var writer = XmlWriter.Create(stream, settings);

        var fechaCreacion = DateTime.Now;
        var messageId = $"MSG-{fechaCreacion:yyyyMMddHHmmss}";
        var paymentInfoId = $"PMT-{fechaCreacion:yyyyMMddHHmmss}";
        var totalAmount = pagos.Sum(p => p.Importe);
        var nbOfTxs = pagos.Count;

        // Document
        writer.WriteStartDocument();
        writer.WriteStartElement("Document", "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03");
        writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");

        // CstmrCdtTrfInitn (Customer Credit Transfer Initiation)
        writer.WriteStartElement("CstmrCdtTrfInitn");

        // GrpHdr (Group Header)
        writer.WriteStartElement("GrpHdr");
        writer.WriteElementString("MsgId", messageId);
        writer.WriteElementString("CreDtTm", fechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"));
        writer.WriteElementString("NbOfTxs", nbOfTxs.ToString());
        writer.WriteElementString("CtrlSum", totalAmount.ToString("F2"));

        // InitgPty (Initiating Party)
        writer.WriteStartElement("InitgPty");
        writer.WriteElementString("Nm", configuracion.NombreEmpresa);
        writer.WriteEndElement(); // InitgPty

        writer.WriteEndElement(); // GrpHdr

        // PmtInf (Payment Information)
        writer.WriteStartElement("PmtInf");
        writer.WriteElementString("PmtInfId", paymentInfoId);
        writer.WriteElementString("PmtMtd", "TRF"); // Transfer

        // PmtTpInf (Payment Type Information)
        writer.WriteStartElement("PmtTpInf");
        writer.WriteStartElement("SvcLvl");
        writer.WriteElementString("Cd", "SEPA");
        writer.WriteEndElement(); // SvcLvl
        writer.WriteEndElement(); // PmtTpInf

        writer.WriteElementString("ReqdExctnDt", fechaCreacion.ToString("yyyy-MM-dd"));

        // Dbtr (Debtor - Empresa)
        writer.WriteStartElement("Dbtr");
        writer.WriteElementString("Nm", configuracion.NombreEmpresa);
        writer.WriteEndElement(); // Dbtr

        // DbtrAcct (Debtor Account)
        writer.WriteStartElement("DbtrAcct");
        writer.WriteStartElement("Id");
        writer.WriteElementString("IBAN", LimpiarIBAN(configuracion.IBAN!));
        writer.WriteEndElement(); // Id
        writer.WriteEndElement(); // DbtrAcct

        // DbtrAgt (Debtor Agent - Banco de la empresa)
        if (!string.IsNullOrEmpty(configuracion.BIC))
        {
            writer.WriteStartElement("DbtrAgt");
            writer.WriteStartElement("FinInstnId");
            writer.WriteElementString("BIC", configuracion.BIC);
            writer.WriteEndElement(); // FinInstnId
            writer.WriteEndElement(); // DbtrAgt
        }

        // CdtTrfTxInf (Credit Transfer Transaction Information) para cada pago
        foreach (var pago in pagos)
        {
            var cliente = pago.Factura.Cliente!;
            var endToEndId = $"E2E-{pago.Id}-{fechaCreacion:yyyyMMdd}";

            writer.WriteStartElement("CdtTrfTxInf");

            // PmtId (Payment Identification)
            writer.WriteStartElement("PmtId");
            writer.WriteElementString("EndToEndId", endToEndId);
            writer.WriteEndElement(); // PmtId

            // Amt (Amount)
            writer.WriteStartElement("Amt");
            writer.WriteStartElement("InstdAmt");
            writer.WriteAttributeString("Ccy", "EUR");
            writer.WriteValue(pago.Importe.ToString("F2"));
            writer.WriteEndElement(); // InstdAmt
            writer.WriteEndElement(); // Amt

            // CdtrAgt (Creditor Agent - Banco del cliente)
            if (!string.IsNullOrEmpty(cliente.BIC))
            {
                writer.WriteStartElement("CdtrAgt");
                writer.WriteStartElement("FinInstnId");
                writer.WriteElementString("BIC", cliente.BIC);
                writer.WriteEndElement(); // FinInstnId
                writer.WriteEndElement(); // CdtrAgt
            }

            // Cdtr (Creditor - Cliente)
            writer.WriteStartElement("Cdtr");
            var nombreCliente = cliente.Tipo == TipoCliente.Particular
                ? $"{cliente.Nombre} {cliente.Apellidos}".Trim()
                : cliente.NombreEmpresa;
            writer.WriteElementString("Nm", nombreCliente);
            writer.WriteEndElement(); // Cdtr

            // CdtrAcct (Creditor Account)
            writer.WriteStartElement("CdtrAcct");
            writer.WriteStartElement("Id");
            writer.WriteElementString("IBAN", LimpiarIBAN(cliente.CuentaBancaria!));
            writer.WriteEndElement(); // Id
            writer.WriteEndElement(); // CdtrAcct

            // RmtInf (Remittance Information)
            writer.WriteStartElement("RmtInf");
            var concepto = $"Pago factura {pago.Factura.NumeroFactura}";
            if (!string.IsNullOrEmpty(pago.Referencia))
                concepto += $" - Ref: {pago.Referencia}";
            writer.WriteElementString("Ustrd", concepto);
            writer.WriteEndElement(); // RmtInf

            writer.WriteEndElement(); // CdtTrfTxInf
        }

        writer.WriteEndElement(); // PmtInf
        writer.WriteEndElement(); // CstmrCdtTrfInitn
        writer.WriteEndElement(); // Document

        writer.WriteEndDocument();
        writer.Flush();

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private string LimpiarIBAN(string iban)
    {
        // Eliminar espacios y convertir a mayúsculas
        return iban.Replace(" ", "").ToUpper();
    }
}
