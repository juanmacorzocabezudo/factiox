using Microsoft.AspNetCore.DataProtection;

namespace FactioX.Services;

public interface ISecretosService
{
    string? Proteger(string? valor);
    string? Desproteger(string? valor);
    bool EstaProtegido(string? valor);
}

public class SecretosService : ISecretosService
{
    private const string Prefijo = "dp1:";
    private readonly IDataProtector _protector;
    private readonly ILogger<SecretosService> _logger;

    public SecretosService(IDataProtectionProvider provider, ILogger<SecretosService> logger)
    {
        _protector = provider.CreateProtector("FactioX.Secretos.v1");
        _logger = logger;
    }

    public bool EstaProtegido(string? valor) => valor != null && valor.StartsWith(Prefijo, StringComparison.Ordinal);

    public string? Proteger(string? valor)
    {
        if (string.IsNullOrEmpty(valor) || EstaProtegido(valor)) return valor;
        return Prefijo + _protector.Protect(valor);
    }

    public string? Desproteger(string? valor)
    {
        // Los valores antiguos sin cifrar se devuelven tal cual hasta que se vuelvan a guardar
        if (!EstaProtegido(valor)) return valor;
        try
        {
            return _protector.Unprotect(valor![Prefijo.Length..]);
        }
        catch (System.Security.Cryptography.CryptographicException ex)
        {
            _logger.LogWarning(ex, "No se pudo descifrar un secreto: es posible que se hayan perdido las claves de protección de datos.");
            return null;
        }
    }
}
