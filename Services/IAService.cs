using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FactioX.Services;

public interface IIAService
{
    Task<string> EnviarMensajeAsync(string mensaje, List<MensajeChat> historial);
    Task<bool> VerificarConexionAsync();
    void SetContextService(IChatContextService contextService);
}

public class MensajeChat
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}

public class IAService : IIAService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _ollamaUrl;
    private readonly string _modelo;
    private IChatContextService? _contextService;

    public IAService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _ollamaUrl = configuration["Ollama:Url"] ?? "http://localhost:11434";
        _modelo = configuration["Ollama:Model"] ?? "llama3.2:1b";
        _httpClient.Timeout = TimeSpan.FromMinutes(2);
    }

    public void SetContextService(IChatContextService contextService)
    {
        _contextService = contextService;
    }

    public async Task<bool> VerificarConexionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ollamaUrl}/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> EnviarMensajeAsync(string mensaje, List<MensajeChat> historial)
    {
        try
        {
            // Obtener contexto de la empresa si está disponible
            string contextoEmpresa = "";
            if (_contextService != null)
            {
                contextoEmpresa = await _contextService.ObtenerContextoEmpresaAsync();
            }

            // Agregar contexto del sistema
            var mensajes = new List<MensajeChat>
            {
                new MensajeChat 
                { 
                    Role = "system", 
                    Content = $@"Eres 'Factu', un asistente virtual especializado en facturación y gestión empresarial. 
Trabajas dentro de FactioX, una aplicación de gestión de facturas, clientes, productos y finanzas.
Ayudas a los usuarios con:
- Consultas sobre facturas, clientes y productos
- Explicaciones de funcionalidades de la aplicación
- Consejos sobre facturación y gestión empresarial
- Respuestas rápidas y precisas
- Interpretación de reportes y estadísticas

{contextoEmpresa}

Sé amigable, conciso y profesional. Usa los datos proporcionados para dar respuestas precisas y útiles.
Responde siempre en español." 
                }
            };

            // Agregar historial
            mensajes.AddRange(historial);

            // Agregar mensaje actual
            mensajes.Add(new MensajeChat { Role = "user", Content = mensaje });

            var requestBody = new
            {
                model = _modelo,
                messages = mensajes,
                stream = false,
                options = new
                {
                    temperature = 0.7,
                    num_predict = 500
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/chat", content);
            
            if (!response.IsSuccessStatusCode)
            {
                return "Lo siento, no pude procesar tu solicitud. ¿Está Ollama ejecutándose?";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaResponse>(responseJson);

            return result?.Message?.Content ?? "No recibí respuesta del modelo.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en IAService: {ex.Message}");
            return $"Error: {ex.Message}. Asegúrate de que Ollama esté ejecutándose y el modelo '{_modelo}' esté disponible.";
        }
    }
}

public class OllamaResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "";
    
    [JsonPropertyName("message")]
    public MensajeChat? Message { get; set; }
    
    [JsonPropertyName("done")]
    public bool Done { get; set; }
}
