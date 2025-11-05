namespace AssetRegistry.Api.Contracts
{
    public class CreateAssetDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Category { get; set; }
        public string? Location { get; set; }
        public DateTime PurchasedAt { get; set; }
    }
}
