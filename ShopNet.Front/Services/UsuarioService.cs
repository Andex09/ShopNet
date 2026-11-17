using Microsoft.AspNetCore.Components.Authorization;
using ShopNet.Front.Auth;
using ShopNet.Front.Services.Contracts;
using ShopNet.Models.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ShopNet.Front.Services
{
    public class UsuarioService: IUsuarioService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authStateProvider;
        public UsuarioService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _authStateProvider = authStateProvider;
        }

        public async Task<List<UsuarioListDto>> GetItems()
        {
            try
            {

                var usuario = await _httpClient.GetFromJsonAsync<List<UsuarioListDto>>("/api/Usuario");

                return usuario;

            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<UsuarioListDto> GetItem(int id)
        {
            try
            {
                var customAuthStateProvider = (AuthStateProvider)_authStateProvider;
                var token = await customAuthStateProvider.GetToken();

                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var usuario = await _httpClient.GetFromJsonAsync<UsuarioListDto>($"/api/Usuario/{id}");

                return usuario;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<UsuarioUpdateDto> UpdateItem(int id, UsuarioUpdateDto usuario)
        {
            try
            {
                await _httpClient.PutAsJsonAsync($"/api/Usuario", usuario);

                return usuario;

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
