using System.Text.Json.Serialization;

namespace AsgardeoMicroservice.Models
{
    public class UserCreateRequest
    {
        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("name")]
        public UserName_? Name { get; set; }

        [JsonPropertyName("emails")]
        public List<UserEmail>? Emails { get; set; }

        [JsonPropertyName("schemas")]
        public List<string>? Schemas { get; set; }
    }

    public class UserName_
    {
        [JsonPropertyName("givenName")]
        public string? GivenName { get; set; }

        [JsonPropertyName("familyName")]
        public string? FamilyName { get; set; }
    }

    public class UserEmail
    {
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class UserResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("userName")]
        public string? UserName { get; set; }

        [JsonPropertyName("name")]
        public UserName_? Name { get; set; }

        [JsonPropertyName("emails")]
        public List<UserEmail>? Emails { get; set; }

        [JsonPropertyName("schemas")]
        public List<string>? Schemas { get; set; }
    }

    public class UsersListResponse
    {
        [JsonPropertyName("totalResults")]
        public int TotalResults { get; set; }

        [JsonPropertyName("Resources")]
        public List<UserResponse>? Resources { get; set; }
    }
}
