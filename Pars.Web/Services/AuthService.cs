using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace Pars.Web.Services;

public record LoginResponse(string Token, string RefreshToken, DateTime ExpiresAt, string Username, string[] Roles);

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _storage;

    public AuthService(HttpClient http, ILocalStorageService storage)
    {
        _http = http;
        _storage = storage;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login",
            new { Username = username, Password = password });

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (result is null) return false;

        await _storage.SetItemAsync("authToken", result.Token);
        await _storage.SetItemAsync("username", result.Username);
        await _storage.SetItemAsync("roles", result.Roles);
        return true;
    }

    public async Task LogoutAsync()
    {
        await _storage.RemoveItemAsync("authToken");
        await _storage.RemoveItemAsync("username");
        await _storage.RemoveItemAsync("roles");
    }

    public async Task<string?> GetTokenAsync() =>
        await _storage.GetItemAsync<string>("authToken");

    public async Task<bool> IsAuthenticatedAsync() =>
        !string.IsNullOrEmpty(await GetTokenAsync());

    public async Task<string[]?> GetRolesAsync() =>
        await _storage.GetItemAsync<string[]>("roles");
}