namespace AsgardeoMicroservice.Models
{
    public class TokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string scope { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; }
    }

    public class Application
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string clientId { get; set; } = string.Empty;
    }

    public class ApplicationsResponse
    {
        public List<Application> applications { get; set; } = new();
        public int totalResults { get; set; }
        public int startIndex { get; set; }
        public int count { get; set; }
    }

    public class CreateApplicationRequest
    {
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string templateId { get; set; } = string.Empty;
    }

    public class UpdateApplicationRequest
    {
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }

    // SCIM 2.0 User Models for /Me endpoint
    public class ScimUser
    {
        public List<string> schemas { get; set; } = new();
        public string id { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public UserName name { get; set; } = new();
        public List<Email> emails { get; set; } = new();
        public ScimMeta meta { get; set; } = new();
        public List<Role> roles { get; set; } = new();
        public EnterpriseUser? enterpriseUser { get; set; }
    }

    public class UserName
    {
        public string givenName { get; set; } = string.Empty;
        public string familyName { get; set; } = string.Empty;
    }

    public class Email
    {
        public string value { get; set; } = string.Empty;
        public bool primary { get; set; }
    }

    public class Role
    {
        public string display { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }

    public class ScimMeta
    {
        public string created { get; set; } = string.Empty;
        public string location { get; set; } = string.Empty;
        public string lastModified { get; set; } = string.Empty;
        public string resourceType { get; set; } = string.Empty;
    }

    public class EnterpriseUser
    {
        public Manager? manager { get; set; }
    }

    public class Manager
    {
        public string value { get; set; } = string.Empty;
        public string displayName { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    {
    public List<string> schemas { get; set; } = new();
    public UserName name { get; set; } = new();
    public string userName { get; set; } = string.Empty;
    public List<Email> emails { get; set; } = new();
    public EnterpriseUser? enterpriseUser { get; set; }

    // Add these for API input mapping
    public string Email { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    }

    public class PatchUserRequest
    {
        public List<string> schemas { get; set; } = new();
        public List<PatchOperation> Operations { get; set; } = new();
    }

    public class PatchOperation
    {
        public string op { get; set; } = string.Empty; // "add", "remove", "replace"
        public string path { get; set; } = string.Empty;
        public object? value { get; set; }
    }

    public class MeSummary
{
    public string? GivenName { get; set; }
    public string? FamilyName { get; set; }
    public string? Email { get; set; }
    public List<string>? RolesDisplayName { get; set; }
}
}
