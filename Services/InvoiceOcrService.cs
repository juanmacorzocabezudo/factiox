using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FactioX.Models;

namespace FactioX.Services;

public interface IInvoiceOcrService
{
    Task<FacturaExtraida> ExtraerDatosFacturaAsync(string imagenBase64);
    Task<bool> VerificarModeloDisponibleAsync();
}

public class InvoiceOcrService : IInvoiceOcrService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _ollamaUrl;
    private readonly string _modeloVision;

    public InvoiceOcrService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _ollamaUrl = configuration["Ollama:Url"] ?? "http://localhost:11434";
        _modeloVision = configuration["Ollama:VisionModel"] ?? "llava";
        _httpClient.Timeout = TimeSpan.FromMinutes(5); // Mayor timeout para procesamiento de imágenes
    }

    public async Task<bool> VerificarModeloDisponibleAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ollamaUrl}/api/tags");
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaModelsResponse>(json);
            
            return result?.Models?.Any(m => m.Name.StartsWith(_modeloVision)) ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<FacturaExtraida> ExtraerDatosFacturaAsync(string imagenBase64)
    {
        try
        {
            // Prompt más simple para reducir carga de memoria
            var prompt = @"Look at this invoice image. Extract the key information and return only JSON:
{""nombreProveedor"":""supplier name"",""nifProveedor"":""tax ID"",""numeroFactura"":""invoice number"",""fechaEmision"":""YYYY-MM-DD"",""total"":123.45}
Only JSON, no text.";

            var requestBody = new
            {
                model = _modeloVision,
                prompt = prompt,
                images = new[] { imagenBase64 },
                stream = false,
                options = new
                {
                    temperature = 0.1,      // Más determinista
                    num_predict = 500,      // Reducir tokens para usar menos memoria
                    num_ctx = 2048,         // Contexto reducido
                    num_gpu = 0             // Forzar CPU si hay problemas de GPU
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            Console.WriteLine($"Llamando a Ollama con modelo: {_modeloVision}");
            Console.WriteLine($"Tamaño imagen base64: {imagenBase64?.Length ?? 0} caracteres");
            
            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status Code: {response.StatusCode}");
            Console.WriteLine($"Response Length: {responseContent?.Length ?? 0}");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error Response: {responseContent}");
                
                // Parsear el error de Ollama para dar mejor feedback
                var errorMsg = "Error en Ollama";
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    if (errorObj != null && errorObj.ContainsKey("error"))
                    {
                        errorMsg = errorObj["error"].ToString() ?? errorMsg;
                        
                        // Detectar error de memoria
                        if (errorMsg.Contains("resource limitations") || errorMsg.Contains("unexpectedly stopped"))
                        {
                            errorMsg = "Ollama se quedó sin memoria. Intenta con una imagen más pequeña (500KB-1MB) o reinicia Ollama.";
                        }
                    }
                }
                catch { }
                
                throw new Exception($"Error en la llamada a Ollama: {response.StatusCode}. {errorMsg}");
            }

            var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(responseContent);

            if (string.IsNullOrEmpty(ollamaResponse?.Response))
            {
                throw new Exception("No se recibió respuesta del modelo de visión");
            }

            Console.WriteLine($"Respuesta del modelo (primeros 500 chars): {ollamaResponse.Response.Substring(0, Math.Min(500, ollamaResponse.Response.Length))}");

            // Limpiar la respuesta para asegurar JSON válido
            var jsonResponse = ollamaResponse.Response.Trim();
            
            // Extraer JSON si está envuelto en texto
            if (jsonResponse.Contains("{") && jsonResponse.Contains("}"))
            {
                var startIndex = jsonResponse.IndexOf('{');
                var endIndex = jsonResponse.LastIndexOf('}') + 1;
                jsonResponse = jsonResponse.Substring(startIndex, endIndex - startIndex);
            }
            
            // Intentar parsear el JSON de la respuesta
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            
            FacturaExtraida? facturaExtraida;
            try
            {
                facturaExtraida = JsonSerializer.Deserialize<FacturaExtraida>(jsonResponse, options);
            }
            catch (JsonException)
            {
                // Si falla el parseo, crear una factura básica con los datos que podamos
                Console.WriteLine("No se pudo parsear como JSON estructurado, creando factura básica");
                facturaExtraida = CrearFacturaBasicaDesdeTexto(ollamaResponse.Response);
            }

            if (facturaExtraida == null)
            {
                throw new Exception("No se pudo parsear la respuesta del modelo");
            }

            // Validar y ajustar datos
            ValidarYAjustarDatos(facturaExtraida);

            return facturaExtraida;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error al parsear JSON: {ex.Message}");
            return CrearFacturaConError($"Error al interpretar los datos extraídos: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en ExtraerDatosFacturaAsync: {ex.Message}");
            return CrearFacturaConError($"Error al procesar la imagen: {ex.Message}");
        }
    }

    private void ValidarYAjustarDatos(FacturaExtraida factura)
    {
        // Validar que haya al menos un nombre de proveedor
        if (string.IsNullOrWhiteSpace(factura.NombreProveedor))
        {
            factura.Advertencias.Add("No se pudo extraer el nombre del proveedor");
        }

        // Validar fechas
        if (factura.FechaEmision == null)
        {
            factura.Advertencias.Add("No se pudo extraer la fecha de emisión");
        }

        // Validar líneas
        if (!factura.Lineas.Any())
        {
            factura.Advertencias.Add("No se encontraron líneas de factura");
        }

        // Validar totales
        if (factura.Total == null || factura.Total <= 0)
        {
            factura.Advertencias.Add("No se pudo extraer el importe total");
        }

        // Calcular totales si faltan
        if (factura.Lineas.Any() && factura.BaseImponible == null)
        {
            factura.BaseImponible = factura.Lineas.Sum(l => l.PrecioUnitario * l.Cantidad * (1 - (l.Descuento ?? 0) / 100));
        }

        // Si no hay IVA especificado, usar 21% por defecto
        if (factura.PorcentajeIVA == null && factura.BaseImponible > 0)
        {
            factura.PorcentajeIVA = 21;
        }

        // Calcular importe IVA si falta
        if (factura.ImporteIVA == null && factura.BaseImponible > 0 && factura.PorcentajeIVA > 0)
        {
            factura.ImporteIVA = factura.BaseImponible * (factura.PorcentajeIVA / 100);
        }

        // Calcular total si falta
        if (factura.Total == null && factura.BaseImponible > 0)
        {
            factura.Total = factura.BaseImponible + (factura.ImporteIVA ?? 0);
        }

        // Ajustar confianza basándose en advertencias
        if (factura.Advertencias.Any())
        {
            factura.Confianza = Math.Max(0, factura.Confianza - (factura.Advertencias.Count * 15));
        }
    }

    private FacturaExtraida CrearFacturaConError(string mensaje)
    {
        return new FacturaExtraida
        {
            Confianza = 0,
            Advertencias = new List<string> { mensaje }
        };
    }

    private FacturaExtraida CrearFacturaBasicaDesdeTexto(string texto)
    {
        return new FacturaExtraida
        {
            NombreProveedor = "Revisar manualmente",
            Confianza = 30,
            Advertencias = new List<string> 
            { 
                "No se pudo extraer la información en formato estructurado.",
                "Por favor, revisa y completa manualmente todos los campos.",
                $"Texto extraído: {texto.Substring(0, Math.Min(200, texto.Length))}..."
            }
        };
    }
}

// Clases para deserialización de respuestas de Ollama
internal class OllamaModelsResponse
{
    [JsonPropertyName("models")]
    public List<OllamaModel>? Models { get; set; }
}

internal class OllamaModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

internal class OllamaGenerateResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";
    
    [JsonPropertyName("response")]
    public string Response { get; set; } = "";
    
    [JsonPropertyName("done")]
    public bool Done { get; set; }
}
