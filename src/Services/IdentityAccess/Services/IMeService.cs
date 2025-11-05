using AsgardeoMicroservice.Models;

namespace AsgardeoMicroservice.Services
{
    public interface IMeService
    {
    Task<MeSummary> GetCurrentUserAsync();
    Task<MeSummary> UpdateCurrentUserAsync(UpdateUserRequest request);
    Task<MeSummary> PatchCurrentUserAsync(PatchUserRequest request);
    }
}