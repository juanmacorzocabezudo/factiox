using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace FactioX.Services;

public interface IEmailService
{
    Task EnviarEmailConAdjunto(string destinatario, string asunto, string cuerpo, byte[] adjunto, string nombreAdjunto, string? cc = null);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarEmailConAdjunto(string destinatario, string asunto, string cuerpo, byte[] adjunto, string nombreAdjunto, string? cc = null)
    {
        try
        {
            // Leer configuración SMTP
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "FactioX";
            var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
            {
                throw new InvalidOperationException("La configuración de email no está completa en appsettings.json");
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail ?? smtpUser, fromName),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };

            message.To.Add(destinatario);

            // Añadir CC si existe
            if (!string.IsNullOrEmpty(cc))
            {
                message.CC.Add(cc);
            }

            // Añadir adjunto
            using var stream = new MemoryStream(adjunto);
            var attachment = new Attachment(stream, nombreAdjunto, "application/pdf");
            message.Attachments.Add(attachment);

            await client.SendMailAsync(message);

            _logger.LogInformation($"Email enviado correctamente a {destinatario}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar email a {destinatario}: {ex.Message}");
            throw new InvalidOperationException($"No se pudo enviar el email: {ex.Message}", ex);
        }
    }
}
