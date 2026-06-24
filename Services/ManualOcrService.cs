using System;
using System.IO;
using System.Drawing;
using Tesseract;
using FactioX.Models;

namespace FactioX.Services;

/// <summary>
/// Servicio para extracción manual de datos mediante OCR en áreas seleccionadas
/// </summary>
public interface IManualOcrService
{
    /// <summary>
    /// Extrae texto de un área específica de una imagen
    /// </summary>
    Task<string> ExtraerTextoDeAreaAsync(string imagenBase64, int x, int y, int width, int height);
    
    /// <summary>
    /// Extrae todos los datos de una factura basándose en áreas seleccionadas
    /// </summary>
    Task<FacturaExtraida> ExtraerDatosDeAreasAsync(string imagenBase64, Dictionary<string, Rectangle> areas);
    
    /// <summary>
    /// Verifica si Tesseract está disponible
    /// </summary>
    bool VerificarTesseractDisponible();
}

public class ManualOcrService : IManualOcrService
{
    private readonly string _tessDataPath;
    private readonly ILogger<ManualOcrService> _logger;

    public ManualOcrService(IConfiguration configuration, ILogger<ManualOcrService> logger)
    {
        _logger = logger;
        // Buscar tessdata en múltiples ubicaciones
        var possiblePaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"),
            Path.Combine(Directory.GetCurrentDirectory(), "tessdata"),
            "/usr/share/tesseract-ocr/4.00/tessdata",
            "/usr/share/tessdata",
            "/opt/homebrew/share/tessdata"
        };

        _tessDataPath = possiblePaths.FirstOrDefault(Directory.Exists) 
            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

        // Crear directorio si no existe
        if (!Directory.Exists(_tessDataPath))
        {
            Directory.CreateDirectory(_tessDataPath);
            _logger.LogWarning($"Directorio tessdata creado en: {_tessDataPath}");
            _logger.LogWarning("Descarga el archivo spa.traineddata desde: https://github.com/tesseract-ocr/tessdata");
        }
    }

    public bool VerificarTesseractDisponible()
    {
        try
        {
            // Verificar que existe el archivo de idioma español
            var trainedDataPath = Path.Combine(_tessDataPath, "spa.traineddata");
            var engTrainedDataPath = Path.Combine(_tessDataPath, "eng.traineddata");
            
            return File.Exists(trainedDataPath) || File.Exists(engTrainedDataPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar disponibilidad de Tesseract");
            return false;
        }
    }

    public async Task<string> ExtraerTextoDeAreaAsync(string imagenBase64, int x, int y, int width, int height)
    {
        return await Task.Run(() =>
        {
            try
            {
                // Convertir base64 a imagen
                var imageBytes = Convert.FromBase64String(imagenBase64);
                using var ms = new MemoryStream(imageBytes);
                using var bitmap = new Bitmap(ms);

                // Recortar el área especificada
                var rect = new Rectangle(x, y, width, height);
                using var croppedBitmap = bitmap.Clone(rect, bitmap.PixelFormat);

                // Guardar temporalmente para Tesseract
                var tempPath = Path.Combine(Path.GetTempPath(), $"ocr_temp_{Guid.NewGuid()}.png");
                croppedBitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

                try
                {
                    // Procesar con Tesseract
                    using var engine = new TesseractEngine(_tessDataPath, "spa+eng", EngineMode.Default);
                    using var img = Pix.LoadFromFile(tempPath);
                    using var page = engine.Process(img);
                    
                    var text = page.GetText().Trim();
                    var confidence = page.GetMeanConfidence();
                    
                    _logger.LogInformation($"OCR extraído (confianza: {confidence:P}): {text}");
                    
                    return text;
                }
                finally
                {
                    // Limpiar archivo temporal
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al extraer texto del área ({x}, {y}, {width}, {height})");
                return string.Empty;
            }
        });
    }

    public async Task<FacturaExtraida> ExtraerDatosDeAreasAsync(
        string imagenBase64, 
        Dictionary<string, Rectangle> areas)
    {
        var factura = new FacturaExtraida
        {
            Lineas = new List<LineaFacturaExtraida>(),
            Advertencias = new List<string>()
        };

        try
        {
            // Extraer cada campo según las áreas seleccionadas
            foreach (var area in areas)
            {
                var texto = await ExtraerTextoDeAreaAsync(
                    imagenBase64, 
                    area.Value.X, 
                    area.Value.Y, 
                    area.Value.Width, 
                    area.Value.Height);

                if (string.IsNullOrWhiteSpace(texto))
                {
                    factura.Advertencias.Add($"No se pudo extraer texto del campo: {area.Key}");
                    continue;
                }

                // Asignar el texto al campo correspondiente
                AsignarCampo(factura, area.Key, texto);
            }

            // Calcular confianza basada en campos extraídos
            var camposExtraidos = 0;
            var camposTotales = areas.Count;
            
            if (!string.IsNullOrEmpty(factura.NombreProveedor)) camposExtraidos++;
            if (!string.IsNullOrEmpty(factura.NumeroFactura)) camposExtraidos++;
            if (factura.Total.HasValue) camposExtraidos++;
            if (factura.BaseImponible.HasValue) camposExtraidos++;
            
            factura.Confianza = (int)((camposExtraidos / (double)Math.Max(camposTotales, 4)) * 100);

            _logger.LogInformation($"Extracción manual completada. Confianza: {factura.Confianza}%");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en extracción manual de OCR");
            factura.Advertencias.Add($"Error general: {ex.Message}");
        }

        return factura;
    }

    private void AsignarCampo(FacturaExtraida factura, string campo, string texto)
    {
        texto = texto.Trim();
        
        switch (campo.ToLower())
        {
            case "numero":
            case "numerofactura":
                factura.NumeroFactura = LimpiarTexto(texto);
                break;

            case "proveedor":
            case "nombreproveedor":
                factura.NombreProveedor = LimpiarTexto(texto);
                break;

            case "nif":
            case "cif":
            case "nifproveedor":
                factura.NifProveedor = LimpiarNIF(texto);
                break;

            case "fecha":
            case "fechaemision":
                factura.FechaEmision = ParsearFecha(texto);
                break;

            case "fechavencimiento":
            case "vencimiento":
                factura.FechaVencimiento = ParsearFecha(texto);
                break;

            case "base":
            case "baseimponible":
                factura.BaseImponible = ParsearImporte(texto);
                break;

            case "iva":
            case "porcentajeiva":
                factura.PorcentajeIVA = ParsearImporte(texto);
                break;

            case "importeiva":
                factura.ImporteIVA = ParsearImporte(texto);
                break;

            case "total":
                factura.Total = ParsearImporte(texto);
                break;

            case "direccion":
            case "direccionproveedor":
                factura.DireccionProveedor = LimpiarTexto(texto);
                break;

            case "ciudad":
            case "ciudadproveedor":
                factura.CiudadProveedor = LimpiarTexto(texto);
                break;

            case "codigopostal":
            case "cp":
                factura.CodigoPostalProveedor = LimpiarTexto(texto);
                break;

            default:
                _logger.LogWarning($"Campo desconocido: {campo}");
                break;
        }
    }

    private string LimpiarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        // Reemplazar saltos de línea por espacios
        texto = texto.Replace('\n', ' ').Replace('\r', ' ');
        
        // Eliminar múltiples espacios
        while (texto.Contains("  "))
            texto = texto.Replace("  ", " ");

        return texto.Trim();
    }

    private string LimpiarNIF(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        // Extraer solo caracteres alfanuméricos
        var nif = new string(texto.Where(c => char.IsLetterOrDigit(c)).ToArray());
        return nif.ToUpper();
    }

    private DateTime? ParsearFecha(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;

        // Limpiar texto
        texto = LimpiarTexto(texto);

        // Formatos comunes de fecha
        string[] formats = {
            "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy",
            "d/M/yyyy", "d-M-yyyy", "d.M.yyyy",
            "yyyy/MM/dd", "yyyy-MM-dd",
            "dd/MM/yy", "dd-MM-yy"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(texto, format, 
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, 
                out DateTime fecha))
            {
                return fecha;
            }
        }

        // Intentar parseo general
        if (DateTime.TryParse(texto, out DateTime fechaGeneral))
        {
            return fechaGeneral;
        }

        _logger.LogWarning($"No se pudo parsear la fecha: {texto}");
        return null;
    }

    private decimal? ParsearImporte(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;

        // Limpiar texto: eliminar símbolos de moneda, espacios, etc.
        texto = texto.Trim()
            .Replace("€", "")
            .Replace("$", "")
            .Replace("EUR", "")
            .Replace(" ", "")
            .Replace("%", "");

        // Manejar separadores decimales (coma o punto)
        // Si tiene coma como decimal (formato europeo): 1.234,56 -> 1234.56
        if (texto.Contains(','))
        {
            // Si tiene punto antes de coma, es separador de miles
            if (texto.IndexOf('.') < texto.IndexOf(','))
            {
                texto = texto.Replace(".", "").Replace(",", ".");
            }
            else
            {
                // Solo tiene coma, es decimal
                texto = texto.Replace(",", ".");
            }
        }

        if (decimal.TryParse(texto, 
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture,
            out decimal importe))
        {
            return importe;
        }

        _logger.LogWarning($"No se pudo parsear el importe: {texto}");
        return null;
    }
}
