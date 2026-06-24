using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace FactioX.Services;

public interface IFirmaDigitalService
{
    string FirmarXml(string xmlSinFirmar, string rutaCertificado, string passwordCertificado);
    bool VerificarFirma(string xmlFirmado);
}

public class FirmaDigitalService : IFirmaDigitalService
{
    public string FirmarXml(string xmlSinFirmar, string rutaCertificado, string passwordCertificado)
    {
        try
        {
            Console.WriteLine($"=== FIRMA DIGITAL ===");
            Console.WriteLine($"Ruta certificado: {rutaCertificado}");
            
            // Cargar certificado
            if (!File.Exists(rutaCertificado))
            {
                throw new Exception($"No se encuentra el archivo de certificado: {rutaCertificado}");
            }
            
            var certificado = new X509Certificate2(rutaCertificado, passwordCertificado, X509KeyStorageFlags.Exportable);
            Console.WriteLine($"Certificado cargado: {certificado.Subject}");
            Console.WriteLine($"Válido desde: {certificado.NotBefore} hasta: {certificado.NotAfter}");
            
            if (DateTime.Now < certificado.NotBefore || DateTime.Now > certificado.NotAfter)
            {
                throw new Exception($"El certificado no es válido. Período de validez: {certificado.NotBefore} - {certificado.NotAfter}");
            }
            
            // Cargar XML
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xmlSinFirmar);
            
            // Crear firma
            var signedXml = new SignedXml(xmlDoc) { SigningKey = certificado.GetRSAPrivateKey() };
            
            // Añadir referencia al documento completo
            var reference = new Reference("");
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);
            
            // Añadir información del certificado
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificado));
            signedXml.KeyInfo = keyInfo;
            
            // Computar firma
            signedXml.ComputeSignature();
            
            // Obtener elemento de firma
            var firmaElement = signedXml.GetXml();
            
            // Insertar la firma antes del elemento de cierre del root
            var rootElement = xmlDoc.DocumentElement;
            if (rootElement != null)
            {
                rootElement.AppendChild(xmlDoc.ImportNode(firmaElement, true));
            }
            
            var xmlFirmado = xmlDoc.OuterXml;
            Console.WriteLine($"✅ XML firmado correctamente. Longitud: {xmlFirmado.Length}");
            
            return xmlFirmado;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al firmar XML: {ex.Message}");
            throw new Exception($"Error al firmar el XML: {ex.Message}");
        }
    }
    
    public bool VerificarFirma(string xmlFirmado)
    {
        try
        {
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(xmlFirmado);
            
            var signedXml = new SignedXml(xmlDoc);
            var nodeList = xmlDoc.GetElementsByTagName("Signature", "http://www.w3.org/2000/09/xmldsig#");
            
            if (nodeList.Count == 0)
            {
                return false;
            }
            
            signedXml.LoadXml((XmlElement)nodeList[0]!);
            
            return signedXml.CheckSignature();
        }
        catch
        {
            return false;
        }
    }
}
