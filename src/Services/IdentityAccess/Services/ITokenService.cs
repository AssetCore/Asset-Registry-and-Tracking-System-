namespace AsgardeoMicroservice.Services
{
    public interface ITokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}
