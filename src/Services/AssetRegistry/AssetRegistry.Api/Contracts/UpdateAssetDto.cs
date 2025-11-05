namespace AssetRegistry.Api.Contracts
{
    public class UpdateAssetDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Category { get; set; }
        public string? Location { get; set; }
        public DateTime? PurchasedAt { get; set; }

    }
}
