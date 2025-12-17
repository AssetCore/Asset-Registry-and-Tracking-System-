using AssetRegistry.Domain.Entities;

namespace AssetRegistry.Application.Assets
{
    public interface IAssetService
    {
        Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Asset>> ListAsync(int page, int pageSize, CancellationToken cancellationToken);

        Task<Guid> CreateAsync(Asset asset, string? userIdentity, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(Asset asset, string? userIdentity, CancellationToken cancellationToken);

        Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken);
    }
}