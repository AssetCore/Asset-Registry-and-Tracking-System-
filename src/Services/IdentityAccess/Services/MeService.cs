using System.Text.Json; 
using System.Text; 
using AsgardeoMicroservice.Configuration;
using Microsoft.Extensions.Options;
using AsgardeoMicroservice.Models;
using System.Net.Http;
using System.Net.Http.Headers; 


namespace AsgardeoMicroservice.Services
{
    public class MeService : IMeService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AsgardeoSettings _settings;
        private readonly ILogger<MeService> _logger;

        public MeService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<AsgardeoSettings> settings,
            ILogger<MeService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<MeSummary> GetCurrentUserAsync()
        {
            SetAuthorizationHeaderFromRequest();
            
            var response = await _httpClient.GetAsync($"scim2/Me");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            var summary = new MeSummary();
            if (root.TryGetProperty("name", out var nameProp))
            {
                summary.GivenName = nameProp.GetProperty("givenName").GetString();
                summary.FamilyName = nameProp.GetProperty("familyName").GetString();
            }
            if (root.TryGetProperty("emails", out var emailsProp) && emailsProp.ValueKind == JsonValueKind.Array && emailsProp.GetArrayLength() > 0)
            {
                summary.Email = emailsProp[0].GetString();
            }
            if (root.TryGetProperty("roles", out var rolesProp) && rolesProp.ValueKind == JsonValueKind.Array)
            {
                summary.RolesDisplayName = new List<string>();
                foreach (var role in rolesProp.EnumerateArray())
                {
                    if (role.TryGetProperty("display", out var displayProp))
                    {
                        summary.RolesDisplayName.Add(displayProp.GetString() ?? "");
                    }
                }
            }
            return summary;
        }

        public async Task<MeSummary> UpdateCurrentUserAsync(UpdateUserRequest request)
        {
            SetAuthorizationHeaderFromRequest();

            // Construct SCIM-compliant user object
            var scimUser = new
            {
                name = new
                {
                    givenName = request.GivenName,
                    familyName = request.FamilyName
                },
                emails = new[]
                {
                    new { value = request.Email, type = "work", primary = true }
                },
                userName = request.userName,
                // Add other required SCIM fields here if needed
            };

            var json = JsonSerializer.Serialize(scimUser, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/scim+json");

            var response = await _httpClient.PutAsync($"scim2/Me", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;
            var summary = new MeSummary();
            if (root.TryGetProperty("name", out var nameProp))
            {
                summary.GivenName = nameProp.GetProperty("givenName").GetString();
                summary.FamilyName = nameProp.GetProperty("familyName").GetString();
            }
            if (root.TryGetProperty("emails", out var emailsProp) && emailsProp.ValueKind == JsonValueKind.Array && emailsProp.GetArrayLength() > 0)
            {
                summary.Email = emailsProp[0].GetProperty("value").GetString();
            }
            if (root.TryGetProperty("roles", out var rolesProp) && rolesProp.ValueKind == JsonValueKind.Array)
            {
                summary.RolesDisplayName = new List<string>();
                foreach (var role in rolesProp.EnumerateArray())
                {
                    if (role.TryGetProperty("display", out var displayProp))
                    {
                        summary.RolesDisplayName.Add(displayProp.GetString() ?? "");
                    }
                }
            }
            return summary;
        }

        public async Task<MeSummary> PatchCurrentUserAsync(PatchUserRequest minimalRequest)
        {
            SetAuthorizationHeaderFromRequest();

            // Construct PatchUserRequest from minimal input
            var patchRequest = new PatchUserRequest
            {
                schemas = new List<string> { "urn:ietf:params:scim:api:messages:2.0:PatchOp" },
                Operations = minimalRequest.Operations
            };

            var json = JsonSerializer.Serialize(patchRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/scim+json");

            var response = await _httpClient.PatchAsync($"scim2/Me", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            using var doc2 = JsonDocument.Parse(responseContent);
            var root2 = doc2.RootElement;
            var summary = new MeSummary();
            if (root2.TryGetProperty("name", out var nameProp))
            {
                summary.GivenName = nameProp.GetProperty("givenName").GetString();
                summary.FamilyName = nameProp.GetProperty("familyName").GetString();
            }
            if (root2.TryGetProperty("emails", out var emailsProp) && emailsProp.ValueKind == JsonValueKind.Array && emailsProp.GetArrayLength() > 0)
            {
                summary.Email = emailsProp[0].GetProperty("value").GetString();
            }
            if (root2.TryGetProperty("roles", out var rolesProp) && rolesProp.ValueKind == JsonValueKind.Array)
            {
                summary.RolesDisplayName = new List<string>();
                foreach (var role in rolesProp.EnumerateArray())
                {
                    if (role.TryGetProperty("display", out var displayProp))
                    {
                        summary.RolesDisplayName.Add(displayProp.GetString() ?? "");
                    }
                }
            }
            return summary;
        }

        private void SetAuthorizationHeaderFromRequest()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                throw new UnauthorizedAccessException("No valid JWT token found in request");
            }
        }
    }
}