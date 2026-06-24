using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using FactioX.Models;

namespace FactioX.Services
{
    public class ResultadoLogin
    {
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
    }

    public interface IAuthService
    {
        bool EstaAutenticado { get; }
        string? UsuarioActual { get; }
        int? UsuarioId { get; }
        string? Rol { get; }
        int? EmpresaId { get; }
        List<int> EmpresaIds { get; } // Para Asesorías con múltiples empresas
        event Action? OnCambioAutenticacion;
        
        Task<ResultadoLogin> IniciarSesionAsync(string usuario, string contrasena);
        Task CerrarSesionAsync();
        Task InicializarAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IEmpresaService _empresaService;
        private readonly ProtectedSessionStorage _sessionStorage;

        public bool EstaAutenticado { get; private set; }
        public string? UsuarioActual { get; private set; }
        public int? UsuarioId { get; private set; }
        public string? Rol { get; private set; }
        public int? EmpresaId { get; private set; }
        public List<int> EmpresaIds { get; private set; } = new List<int>();
        public event Action? OnCambioAutenticacion;

        public AuthService(
            IUsuarioService usuarioService,
            IEmpresaService empresaService,
            ProtectedSessionStorage sessionStorage)
        {
            _usuarioService = usuarioService;
            _empresaService = empresaService;
            _sessionStorage = sessionStorage;
        }

        public async Task InicializarAsync()
        {
            try
            {
                var authResult = await _sessionStorage.GetAsync<bool>("isAuthenticated");
                
                if (authResult.Success && authResult.Value)
                {
                    var usernameResult = await _sessionStorage.GetAsync<string>("username");
                    var userIdResult = await _sessionStorage.GetAsync<int>("userId");
                    var rolResult = await _sessionStorage.GetAsync<string>("userRole");
                    var empresaIdResult = await _sessionStorage.GetAsync<int>("EmpresaId");
                    var empresaIdsResult = await _sessionStorage.GetAsync<List<int>>("EmpresaIds");

                    if (usernameResult.Success)
                    {
                        EstaAutenticado = true;
                        UsuarioActual = usernameResult.Value;
                        UsuarioId = userIdResult.Success ? userIdResult.Value : null;
                        Rol = rolResult.Success ? rolResult.Value : null;
                        EmpresaId = empresaIdResult.Success ? empresaIdResult.Value : null;
                        EmpresaIds = empresaIdsResult.Success && empresaIdsResult.Value != null 
                            ? empresaIdsResult.Value 
                            : new List<int>();
                        OnCambioAutenticacion?.Invoke();
                    }
                }
            }
            catch
            {
                // Si hay error, mantener estado no autenticado
            }
        }

        public async Task<ResultadoLogin> IniciarSesionAsync(string usuario, string contrasena)
        {
            var usuarioDb = await _usuarioService.ValidarCredencialesAsync(usuario, contrasena);
            
            if (usuarioDb != null)
            {
                // Verificar si el usuario ha caducado
                if (usuarioDb.FechaCaducidad.HasValue && usuarioDb.FechaCaducidad.Value < DateTime.Now)
                {
                    var diasCaducado = (DateTime.Now - usuarioDb.FechaCaducidad.Value).Days;
                    return new ResultadoLogin 
                    { 
                        Exitoso = false, 
                        MensajeError = $"Tu acceso de prueba caducó el {usuarioDb.FechaCaducidad.Value:dd/MM/yyyy}. Contacta con el administrador para renovarlo."
                    };
                }
                
                List<int> empresaIds = new List<int>();
                
                // Si es Asesoría, cargar las empresas asociadas
                if (usuarioDb.Rol == Roles.Asesoria)
                {
                    var empresasAsociadas = await _usuarioService.ObtenerEmpresasAsociadasAsync(usuarioDb.Id);
                    empresaIds = empresasAsociadas.Select(e => e.EmpresaId).ToList();
                    
                    // Validar que tiene al menos una empresa asociada
                    if (!empresaIds.Any())
                    {
                        return new ResultadoLogin { Exitoso = false, MensajeError = "No tienes empresas asociadas. Contacta con el administrador." };
                    }
                }
                // Validar que la empresa está activa si no es SuperAdmin ni Asesoría
                else if (usuarioDb.Rol != Roles.SuperAdministrador && usuarioDb.EmpresaId.HasValue)
                {
                    var empresa = await _empresaService.ObtenerPorIdAsync(usuarioDb.EmpresaId.Value);
                    if (empresa == null || !empresa.Activa)
                    {
                        return new ResultadoLogin { Exitoso = false, MensajeError = "La empresa está desactivada. Contacta con el administrador." };
                    }
                    empresaIds.Add(usuarioDb.EmpresaId.Value);
                }

                // Guardar en sesión
                await _sessionStorage.SetAsync("isAuthenticated", true);
                await _sessionStorage.SetAsync("username", usuarioDb.Username);
                await _sessionStorage.SetAsync("userId", usuarioDb.Id);
                await _sessionStorage.SetAsync("userRole", usuarioDb.Rol);
                await _sessionStorage.SetAsync("EmpresaIds", empresaIds);
                
                if (usuarioDb.EmpresaId.HasValue)
                {
                    await _sessionStorage.SetAsync("EmpresaId", usuarioDb.EmpresaId.Value);
                }

                // Actualizar estado local
                EstaAutenticado = true;
                UsuarioActual = usuarioDb.Username;
                UsuarioId = usuarioDb.Id;
                Rol = usuarioDb.Rol;
                EmpresaId = usuarioDb.EmpresaId;
                EmpresaIds = empresaIds;
                
                OnCambioAutenticacion?.Invoke();
                return new ResultadoLogin { Exitoso = true };
            }

            return new ResultadoLogin { Exitoso = false, MensajeError = "Usuario o contraseña incorrectos." };
        }

        public async Task CerrarSesionAsync()
        {
            await _sessionStorage.DeleteAsync("isAuthenticated");
            await _sessionStorage.DeleteAsync("username");
            await _sessionStorage.DeleteAsync("userId");
            await _sessionStorage.DeleteAsync("userRole");
            await _sessionStorage.DeleteAsync("EmpresaId");
            await _sessionStorage.DeleteAsync("EmpresaIds");

            EstaAutenticado = false;
            UsuarioActual = null;
            UsuarioId = null;
            Rol = null;
            EmpresaId = null;
            EmpresaIds = new List<int>();
            
            OnCambioAutenticacion?.Invoke();
        }
    }
}
