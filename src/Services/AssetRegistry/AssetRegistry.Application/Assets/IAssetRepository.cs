using AssetRegistry.Domain.Entities;

public interface IAssetRepository
{
    Task<Asset?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Asset?> GetByCodeAsync(string code, CancellationToken ct);
    Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken ct);
    Task AddAsync(Asset asset, CancellationToken ct);
    Task UpdateAsync(Asset asset, CancellationToken ct);
    Task<bool> DeactivateAsync(Guid id, CancellationToken ct);
    Task<bool> RestoreAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}