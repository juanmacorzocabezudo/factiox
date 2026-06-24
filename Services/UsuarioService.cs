using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace FactioX.Services;

public interface IUsuarioService
{
    Task<List<Usuario>> ObtenerTodosAsync();
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<Usuario?> ObtenerPorUsernameAsync(string username);
    Task<Usuario?> ValidarCredencialesAsync(string username, string password);
    Task<Usuario> CrearAsync(Usuario usuario);
    Task ActualizarAsync(int id, Usuario usuario);
    Task<bool> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva);
    Task<string> ResetearPasswordAsync(int usuarioId);
    Task EliminarAsync(int id);
    Task<List<UsuarioEmpresa>> ObtenerEmpresasAsociadasAsync(int usuarioId);
    Task AsociarEmpresaAsync(int usuarioId, int empresaId);
    Task DesasociarEmpresaAsync(int usuarioId, int empresaId);
}

public class UsuarioService : IUsuarioService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public UsuarioService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Usuarios
            .Include(u => u.Empresa)
            .ToListAsync();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Usuarios
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> ObtenerPorUsernameAsync(string username)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Usuarios
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string username, string password)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Username == username && u.Activo);
        
        if (usuario == null)
            return null;
        
        // Verificar si la contraseña es hash o texto plano (para migración)
        bool esHashValido = usuario.Password.StartsWith("$2") 
            ? BCrypt.Net.BCrypt.Verify(password, usuario.Password)
            : usuario.Password == password;
        
        return esHashValido ? usuario : null;
    }

    public async Task<Usuario> CrearAsync(Usuario usuario)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        usuario.FechaCreacion = DateTime.Now;
        
        // Guardar contraseña en texto plano (para superadmin)
        if (!usuario.Password.StartsWith("$2"))
        {
            usuario.PasswordTextoPlano = usuario.Password;
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);
        }
        
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
    }

    public async Task ActualizarAsync(int id, Usuario usuario)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var existente = await context.Usuarios.FindAsync(id);
        if (existente != null)
        {
            // Guardar la contraseña original para no sobrescribirla
            var passwordOriginal = existente.Password;
            var passwordTextoPlanoOriginal = existente.PasswordTextoPlano;
            
            context.Entry(existente).CurrentValues.SetValues(usuario);
            
            // Restaurar la contraseña original (el cambio de contraseña se hace con CambiarPasswordAsync)
            existente.Password = passwordOriginal;
            existente.PasswordTextoPlano = passwordTextoPlanoOriginal;
            
            // Si se proporciona una nueva contraseña, actualizarla
            if (!string.IsNullOrEmpty(usuario.Password) && !usuario.Password.StartsWith("$2") && usuario.Password != passwordOriginal)
            {
                existente.PasswordTextoPlano = usuario.Password;
                existente.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);
            }
            
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<bool> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios.FindAsync(usuarioId);
        
        if (usuario == null)
            return false;
        
        // Verificar contraseña actual (compatibilidad con texto plano para migración)
        bool passwordCorrecta = usuario.Password.StartsWith("$2")
            ? BCrypt.Net.BCrypt.Verify(passwordActual, usuario.Password)
            : usuario.Password == passwordActual;
        
        if (!passwordCorrecta)
            return false;
        
        // Actualizar con la nueva contraseña hasheada y en texto plano
        usuario.PasswordTextoPlano = passwordNueva;
        usuario.Password = BCrypt.Net.BCrypt.HashPassword(passwordNueva);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<string> ResetearPasswordAsync(int usuarioId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios.FindAsync(usuarioId);
        
        if (usuario == null)
            throw new Exception("Usuario no encontrado");
        
        // Generar una contraseña temporal aleatoria
        var passwordTemporal = GenerarPasswordTemporal();
        
        // Guardar en texto plano y hashear
        usuario.PasswordTextoPlano = passwordTemporal;
        usuario.Password = BCrypt.Net.BCrypt.HashPassword(passwordTemporal);
        await context.SaveChangesAsync();
        
        // Retornar la contraseña en texto plano para que el admin la vea
        return passwordTemporal;
    }

    private string GenerarPasswordTemporal()
    {
        const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(caracteres, 10)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public async Task EliminarAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            context.Usuarios.Remove(usuario);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<UsuarioEmpresa>> ObtenerEmpresasAsociadasAsync(int usuarioId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.UsuarioEmpresas
            .Include(ue => ue.Empresa)
            .Where(ue => ue.UsuarioId == usuarioId && ue.Activo)
            .ToListAsync();
    }
    
    public async Task AsociarEmpresaAsync(int usuarioId, int empresaId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Verificar si ya existe la asociación
        var existente = await context.UsuarioEmpresas
            .FirstOrDefaultAsync(ue => ue.UsuarioId == usuarioId && ue.EmpresaId == empresaId);
        
        if (existente == null)
        {
            var usuarioEmpresa = new UsuarioEmpresa
            {
                UsuarioId = usuarioId,
                EmpresaId = empresaId,
                FechaAsociacion = DateTime.Now,
                Activo = true
            };
            
            context.UsuarioEmpresas.Add(usuarioEmpresa);
            await context.SaveChangesAsync();
        }
        else if (!existente.Activo)
        {
            // Reactivar la asociación si existía pero estaba inactiva
            existente.Activo = true;
            existente.FechaAsociacion = DateTime.Now;
            await context.SaveChangesAsync();
        }
    }
    
    public async Task DesasociarEmpresaAsync(int usuarioId, int empresaId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var usuarioEmpresa = await context.UsuarioEmpresas
            .FirstOrDefaultAsync(ue => ue.UsuarioId == usuarioId && ue.EmpresaId == empresaId);
        
        if (usuarioEmpresa != null)
        {
            usuarioEmpresa.Activo = false;
            await context.SaveChangesAsync();
        }
    }
}
