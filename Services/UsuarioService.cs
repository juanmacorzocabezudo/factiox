using System.Security.Cryptography;
using FactioX.Models;
using FactioX.Data;
using Microsoft.EntityFrameworkCore;

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
    private readonly ITenantService _tenantService;

    public UsuarioService(IDbContextFactory<ApplicationDbContext> contextFactory, ITenantService tenantService)
    {
        _contextFactory = contextFactory;
        _tenantService = tenantService;
    }

    private sealed record Sesion(string? Rol, int? EmpresaId, int? UsuarioId)
    {
        public bool EsSuperAdmin => Rol == Roles.SuperAdministrador;
        public bool EsAdministrador => Rol == Roles.Administrador && EmpresaId.HasValue;
    }

    private async Task<Sesion> ObtenerSesionAsync() =>
        new(await _tenantService.GetRolAsync(), await _tenantService.GetEmpresaIdAsync(), await _tenantService.GetUsuarioIdAsync());

    private static bool PuedeGestionar(Sesion sesion, Usuario objetivo) =>
        sesion.EsSuperAdmin
        || (sesion.EsAdministrador && objetivo.EmpresaId == sesion.EmpresaId
            && objetivo.Rol is Roles.Administrador or Roles.Usuario);

    public async Task<List<Usuario>> ObtenerTodosAsync()
    {
        var sesion = await ObtenerSesionAsync();
        if (!sesion.EsSuperAdmin && !sesion.EsAdministrador)
            return new List<Usuario>();

        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Usuarios.Include(u => u.Empresa).AsQueryable();
        if (!sesion.EsSuperAdmin)
            query = query.Where(u => u.EmpresaId == sesion.EmpresaId);
        return await query.ToListAsync();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        var sesion = await ObtenerSesionAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (usuario == null) return null;
        return usuario.Id == sesion.UsuarioId || PuedeGestionar(sesion, usuario) ? usuario : null;
    }

    public async Task<Usuario?> ObtenerPorUsernameAsync(string username)
    {
        var sesion = await ObtenerSesionAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Username == username);
        if (usuario == null) return null;
        return usuario.Id == sesion.UsuarioId || PuedeGestionar(sesion, usuario) ? usuario : null;
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string username, string password)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Username == username && u.Activo);
        
        if (usuario == null)
            return null;

        if (EsHash(usuario.Password))
            return BCrypt.Net.BCrypt.Verify(password, usuario.Password) ? usuario : null;

        // Contraseña heredada sin cifrar: se compara una vez y se cifra en el acto
        if (!CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(usuario.Password),
                System.Text.Encoding.UTF8.GetBytes(password)))
            return null;

        usuario.Password = BCrypt.Net.BCrypt.HashPassword(password);
        await context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> CrearAsync(Usuario usuario)
    {
        var sesion = await ObtenerSesionAsync();
        if (!sesion.EsSuperAdmin)
        {
            if (!sesion.EsAdministrador)
                throw new UnauthorizedAccessException("No tiene permisos para crear usuarios.");
            if (usuario.Rol is not (Roles.Administrador or Roles.Usuario))
                throw new UnauthorizedAccessException("No tiene permisos para crear usuarios con este rol.");
            usuario.EmpresaId = sesion.EmpresaId;
            usuario.EsAccesoPrueba = false;
            usuario.FechaCaducidad = null;
        }
        if (string.IsNullOrWhiteSpace(usuario.Password))
            throw new InvalidOperationException("La contraseña es obligatoria.");

        using var context = await _contextFactory.CreateDbContextAsync();
        if (await context.Usuarios.AnyAsync(u => u.Username == usuario.Username))
            throw new InvalidOperationException("Ya existe un usuario con ese nombre.");

        usuario.FechaCreacion = DateTime.Now;
        if (!EsHash(usuario.Password))
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);
        
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
    }

    public async Task ActualizarAsync(int id, Usuario usuario)
    {
        var sesion = await ObtenerSesionAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var existente = await context.Usuarios.FindAsync(id);
        if (existente == null) return;

        var esPropio = existente.Id == sesion.UsuarioId;
        var gestiona = PuedeGestionar(sesion, existente);
        if (!gestiona && !esPropio)
            throw new UnauthorizedAccessException("No tiene permisos para modificar este usuario.");

        var passwordOriginal = existente.Password;
        if (sesion.EsSuperAdmin)
        {
            context.Entry(existente).CurrentValues.SetValues(usuario);
        }
        else if (gestiona)
        {
            if (usuario.Rol is not (Roles.Administrador or Roles.Usuario))
                throw new UnauthorizedAccessException("No tiene permisos para asignar este rol.");
            existente.Username = usuario.Username;
            existente.NombreCompleto = usuario.NombreCompleto;
            existente.Email = usuario.Email;
            existente.Rol = usuario.Rol;
            existente.Activo = usuario.Activo;
        }
        else
        {
            // Un usuario solo puede editar sus datos personales, no su rol, empresa ni estado
            existente.NombreCompleto = usuario.NombreCompleto;
            existente.Email = usuario.Email;
        }
        existente.Id = id;
        existente.Password = passwordOriginal;

        if (!string.IsNullOrEmpty(usuario.Password) && !EsHash(usuario.Password) && usuario.Password != passwordOriginal)
        {
            if (!gestiona)
                throw new UnauthorizedAccessException("Use la opción de cambio de contraseña del perfil.");
            existente.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);
        }

        await context.SaveChangesAsync();
    }
    
    public async Task<bool> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva)
    {
        var sesion = await ObtenerSesionAsync();
        if (sesion.UsuarioId != usuarioId)
            throw new UnauthorizedAccessException("Solo puede cambiar su propia contraseña.");

        using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios.FindAsync(usuarioId);
        
        if (usuario == null)
            return false;
        
        bool passwordCorrecta = EsHash(usuario.Password)
            ? BCrypt.Net.BCrypt.Verify(passwordActual, usuario.Password)
            : usuario.Password == passwordActual;
        
        if (!passwordCorrecta)
            return false;
        
        usuario.Password = BCrypt.Net.BCrypt.HashPassword(passwordNueva);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<string> ResetearPasswordAsync(int usuarioId)
    {
        var sesion = await ObtenerSesionAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios.FindAsync(usuarioId)
            ?? throw new InvalidOperationException("Usuario no encontrado");
        if (!PuedeGestionar(sesion, usuario))
            throw new UnauthorizedAccessException("No tiene permisos para restablecer la contraseña de este usuario.");
        
        var passwordTemporal = GenerarPasswordTemporal();
        usuario.Password = BCrypt.Net.BCrypt.HashPassword(passwordTemporal);
        await context.SaveChangesAsync();
        
        // Se devuelve una sola vez para que el administrador la comunique; no se almacena
        return passwordTemporal;
    }

    private static string GenerarPasswordTemporal()
    {
        const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        return RandomNumberGenerator.GetString(caracteres, 12);
    }

    private static bool EsHash(string password) => password.StartsWith("$2");

    public async Task EliminarAsync(int id)
    {
        var sesion = await ObtenerSesionAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios.FindAsync(id);
        if (usuario == null) return;
        if (!PuedeGestionar(sesion, usuario) || usuario.Id == sesion.UsuarioId || usuario.Rol == Roles.SuperAdministrador)
            throw new UnauthorizedAccessException("No tiene permisos para eliminar este usuario.");
        context.Usuarios.Remove(usuario);
        await context.SaveChangesAsync();
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
        await ValidarSuperAdminAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var existente = await context.UsuarioEmpresas
            .FirstOrDefaultAsync(ue => ue.UsuarioId == usuarioId && ue.EmpresaId == empresaId);
        
        if (existente == null)
        {
            context.UsuarioEmpresas.Add(new UsuarioEmpresa
            {
                UsuarioId = usuarioId,
                EmpresaId = empresaId,
                FechaAsociacion = DateTime.Now,
                Activo = true
            });
            await context.SaveChangesAsync();
        }
        else if (!existente.Activo)
        {
            existente.Activo = true;
            existente.FechaAsociacion = DateTime.Now;
            await context.SaveChangesAsync();
        }
    }
    
    public async Task DesasociarEmpresaAsync(int usuarioId, int empresaId)
    {
        await ValidarSuperAdminAsync();
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var usuarioEmpresa = await context.UsuarioEmpresas
            .FirstOrDefaultAsync(ue => ue.UsuarioId == usuarioId && ue.EmpresaId == empresaId);
        
        if (usuarioEmpresa != null)
        {
            usuarioEmpresa.Activo = false;
            await context.SaveChangesAsync();
        }
    }

    private async Task ValidarSuperAdminAsync()
    {
        if (!await _tenantService.IsSuperAdminAsync())
            throw new UnauthorizedAccessException("Solo el SuperAdministrador puede gestionar asociaciones de empresas.");
    }
}
