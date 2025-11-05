using System.Text.Json;
using AsgardeoMicroservice.Models;

namespace AsgardeoMicroservice.Services
{
    public interface IAsgardeoService
    {
        Task SetAuthorizationHeaderAsync();

        Task<string> GetUsersAsync(string? filter = null, string? attributes = null, int? startIndex = null, int? count = null);
        Task<string> GetUserByIdAsync(string id, string? attributes = null);
        Task<string> CreateUserAsync(System.Text.Json.JsonElement userRequest, string? attributes = null);
        Task<string> UpdateUserAsync(string id, System.Text.Json.JsonElement userRequest, string? attributes = null);
        Task<string> PatchUserAsync(string id, System.Text.Json.JsonElement patchRequest, string? attributes = null);
        Task<string> DeleteUserAsync(string id, string? attributes = null);
        Task<string> SearchUsersAsync(System.Text.Json.JsonElement searchRequest);
        Task<string> PatchUserRoleAsync(string roleId, JsonElement patchRequest);
        Task<string> PatchAuditorRoleAsync(JsonElement patchRequest);
        Task<string> PatchAuditUserRoleAsync(JsonElement patchRequest);
    }
}
