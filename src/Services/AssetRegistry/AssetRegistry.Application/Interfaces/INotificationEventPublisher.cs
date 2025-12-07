using AssetRegistry.Domain.Events;

namespace AssetRegistry.Application.Interfaces;

public interface INotificationEventPublisher
{
    Task PublishWarrantyExpiryAsync(AssetWarrantyExpiryEvent evt, CancellationToken ct = default);
    Task PublishMaintenanceDueAsync(AssetMaintenanceDueEvent evt, CancellationToken ct = default);
    Task PublishAssetAssignmentAsync(AssetAssignmentEvent evt, CancellationToken ct = default);
}