using AssetRegistry.Domain.Entities;
using AssetRegistry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetRegistry.Infrastructure.Repositories
{
    public class AssetRepository : IAssetRepository
    {
        private readonly AssetRegistryDbContext _db;

        public AssetRepository(AssetRegistryDbContext db) =>
            _db = db ?? throw new ArgumentNullException(nameof(db));

        public async Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken ct) =>
            await _db.Assets
                     .AsNoTracking()
                     .ToListAsync(ct);

        public Task<Asset?> GetByIdAsync(Guid id, CancellationToken ct) =>
            _db.Assets
               .AsNoTracking()
               .FirstOrDefaultAsync(assetEntity => assetEntity.Id == id, ct);

        public Task<Asset?> GetByCodeAsync(string code, CancellationToken ct) =>
            _db.Assets
               .AsNoTracking()
               .FirstOrDefaultAsync(assetEntity => assetEntity.Code == code, ct);

        public Task AddAsync(Asset asset, CancellationToken ct) =>
            _db.Assets.AddAsync(asset, ct).AsTask();

        public Task UpdateAsync(Asset asset, CancellationToken ct)
        {
            _db.Attach(asset);
            _db.Entry(asset).State = EntityState.Modified;
            return Task.CompletedTask;
        }

        public async Task<bool> DeactivateAsync(Guid id, CancellationToken ct)
        {
            var assetEntity = await _db.Assets
                                       .IgnoreQueryFilters()
                                       .FirstOrDefaultAsync(assetEntity => assetEntity.Id == id, ct);
            if (assetEntity is null) return false;

            assetEntity.IsDeleted = true;
            return true; 
        }

        public async Task<bool> RestoreAsync(Guid id, CancellationToken ct)
        {
            var assetEntity = await _db.Assets
                                       .IgnoreQueryFilters()
                                       .FirstOrDefaultAsync(assetEntity => assetEntity.Id == id, ct);
            if (assetEntity is null) return false;

            assetEntity.IsDeleted = false;
            return true; 
        }

        public Task SaveChangesAsync(CancellationToken ct) =>
            _db.SaveChangesAsync(ct);
    }
}