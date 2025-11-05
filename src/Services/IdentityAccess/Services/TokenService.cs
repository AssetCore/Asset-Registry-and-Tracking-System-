using AsgardeoMicroservice.Configuration;
using AsgardeoMicroservice.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace AsgardeoMicroservice.Services
{
    public class TokenService : ITokenService
    {
        private readonly HttpClient _httpClient;
        private readonly AsgardeoSettings _settings;
        private string? _cachedToken;
        private DateTime _tokenExpiry = DateTime.MinValue;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public TokenService(HttpClient httpClient, IOptions<AsgardeoSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                // Return cached token if still valid (with 5 minute buffer)
                if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
                {
                    return _cachedToken;
                }

                // Get new token
                var tokenUrl = $"https://api.asgardeo.io/t/{_settings.OrganizationName}/oauth2/token";
                
                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

                var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", _settings.Scope)
                });

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenResponse?.access_token == null)
                {
                    throw new InvalidOperationException("Failed to retrieve access token");
                }

                _cachedToken = tokenResponse.access_token;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);

                return _cachedToken;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
