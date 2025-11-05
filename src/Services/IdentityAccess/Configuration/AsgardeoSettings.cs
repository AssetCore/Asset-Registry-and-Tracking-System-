namespace AsgardeoMicroservice.Configuration
{
    public class AsgardeoSettings
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string AuditorRoleId { get; set; } = string.Empty;
        public string UserRoleId { get; set; } = string.Empty;
    }
}
