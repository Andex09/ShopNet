using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using ShopNet.Front.Extensions;
using ShopNet.Front.Models;
using ShopNet.Models.Dtos;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;


namespace ShopNet.Front.Auth
{
    public class AuthStateProvider: AuthenticationStateProvider
    {
        private readonly ISessionStorageService _sessionStorageService;
        private readonly HttpClient _httpClient;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public AuthStateProvider(ISessionStorageService sessionStorageService, HttpClient httpClient)
        {
            _sessionStorageService = sessionStorageService;
            _httpClient = httpClient;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var sesionUsuario = await _sessionStorageService.ReadEncryptedItemAsync<SesionUsuario>("sesionUsuario");
                
                if (sesionUsuario == null)
                    return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(_anonymous)));
                
                var identity = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, sesionUsuario.usuario.UsuNombre),
                    new Claim(ClaimTypes.Role, sesionUsuario.usuario.UsuRol)
                }, "JwtAuth");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", sesionUsuario.token);

                return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
            }
            catch (Exception)
            {
                return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(_anonymous)));
            }
        }

        public async Task UpdateAuthenticationState(SesionUsuario? sesionUsuario)
        {
            ClaimsPrincipal claimsPrincipal;

            if (sesionUsuario != null)
            {
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, sesionUsuario.usuario.UsuNombre),
                    new Claim(ClaimTypes.Role, sesionUsuario.usuario.UsuRol)
                }));

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", sesionUsuario.token);
                await _sessionStorageService.SaveItemEncryptedAsync<SesionUsuario>("sesionUsuario", sesionUsuario);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;

                claimsPrincipal = _anonymous;
                await _sessionStorageService.RemoveItemAsync("sesionUsuario");
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task<string> GetToken()
        {
            var token = string.Empty;

            try
            {
                var sesionUsuario = await _sessionStorageService.ReadEncryptedItemAsync<SesionUsuario>("sesionUsuario");
                if (sesionUsuario != null)
                     token = sesionUsuario.token;


            }
            catch (Exception)
            {
                
            }

            return token;

        }

        public async Task<int> GetUserId()
        {
            int userId = 0;

            try
            {
                var sesionUsuario = await _sessionStorageService.ReadEncryptedItemAsync<SesionUsuario>("sesionUsuario");
                if (sesionUsuario != null)
                    userId = sesionUsuario.usuario.UsuID;


            }
            catch (Exception)
            {

            }

            return userId;

        }

    }
}
