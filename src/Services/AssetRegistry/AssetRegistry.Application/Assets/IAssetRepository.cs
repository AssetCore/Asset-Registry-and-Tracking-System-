using AssetRegistry.Domain.Entities;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetRegistry.Application.Assets
{
    public interface IAssetRepository
    {
        Task<Asset?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Asset?> GetByCodeAsync(string code, CancellationToken ct);
        Task<List<Asset>> GetAllAsync(CancellationToken ct);
        Task AddAsync(Asset asset, CancellationToken ct);
        Task UpdateAsync(Asset asset, CancellationToken ct);       
        Task<bool> RestoreAsync(Guid id, CancellationToken ct);     
        Task SaveChangesAsync(CancellationToken ct);
    }
}
