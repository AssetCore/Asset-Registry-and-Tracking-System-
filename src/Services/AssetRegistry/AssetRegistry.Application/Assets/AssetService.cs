using AssetRegistry.Domain.Entities;
using AssetRegistry.Application.Interfaces;
using AssetRegistry.Domain.Events;

namespace AssetRegistry.Application.Assets
{
    public sealed class AssetService : IAssetService
    {
        private readonly IAssetRepository _assetRepository;
        private readonly INotificationEventPublisher _notificationEventPublisher;

        public AssetService(
            IAssetRepository assetRepository,
            INotificationEventPublisher notificationEventPublisher)
        {
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _notificationEventPublisher = notificationEventPublisher ?? throw new ArgumentNullException(nameof(notificationEventPublisher));
        }

        public Task<Asset?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            _assetRepository.GetByIdAsync(id, cancellationToken);

        public async Task<IReadOnlyList<Asset>> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var allAssets = await _assetRepository.GetAllAsync(cancellationToken);
            return allAssets
                .OrderBy(assetEntity => assetEntity.Name)
                .Take(pageSize)
                .ToList();
        }

        public async Task<Guid> CreateAsync(Asset asset, string? userIdentity, CancellationToken cancellationToken)
        {
            if (asset.Id == Guid.Empty) asset.Id = Guid.NewGuid();

            if (string.IsNullOrWhiteSpace(asset.Code))
                throw new ArgumentException("Code is required.", nameof(asset));

            if (string.IsNullOrWhiteSpace(asset.Name))
                throw new ArgumentException("Name is required.", nameof(asset));

            var existing = await _assetRepository.GetByCodeAsync(asset.Code, cancellationToken);
            if (existing is not null)
                throw new InvalidOperationException("Asset code already exists.");

            asset.CreatedBy = userIdentity;
            asset.CreatedAt = DateTime.UtcNow;

            await _assetRepository.AddAsync(asset, cancellationToken);
            await _assetRepository.SaveChangesAsync(cancellationToken);

            var assignmentEvent = new AssetAssignmentEvent
            {
                AssetId = asset.Id,
                AssetName = asset.Name,
                NewOwnerEmail = string.Empty,   
                NewOwnerName = string.Empty,    
                SlackChannel = null,            
                AssignmentDate = DateTime.UtcNow
            };

            await _notificationEventPublisher.PublishAssetAssignmentAsync(
                assignmentEvent,
                cancellationToken);

            return asset.Id;
        }

        public async Task<bool> UpdateAsync(Asset asset, string? userIdentity, CancellationToken cancellationToken)
        {
            var current = await _assetRepository.GetByIdAsync(asset.Id, cancellationToken);
            if (current is null) return false;

            if (!string.Equals(current.Code, asset.Code, StringComparison.Ordinal))
            {
                var duplicate = await _assetRepository.GetByCodeAsync(asset.Code, cancellationToken);
                if (duplicate is not null && duplicate.Id != asset.Id)
                    throw new InvalidOperationException("Asset code already exists.");
            }

            asset.UpdatedBy = userIdentity;
            asset.UpdatedAt = DateTime.UtcNow;

            await _assetRepository.UpdateAsync(asset, cancellationToken);
            await _assetRepository.SaveChangesAsync(cancellationToken);


            return true;
        }

        public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken)
        {
            var changed = await _assetRepository.DeactivateAsync(id, cancellationToken);
            if (!changed) return false;

            await _assetRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
        {
            var changed = await _assetRepository.RestoreAsync(id, cancellationToken);
            if (!changed) return false;

            await _assetRepository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}