namespace AssetRegistry.Api.Contracts
{
    public class AssetResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Category { get; set; }
        public string? Location { get; set; }
        public DateTime PurchasedAt { get; set; }
        public bool IsDeleted { get; set; }
    }
}
