using System.Text.Json.Serialization;

namespace ZoomagazinApi.Models;

public class JwtSettings
{
    [JsonPropertyName("Key")]
    public string Key { get; set; }

    [JsonPropertyName("Issuer")]
    public string Issuer { get; set; }

    [JsonPropertyName("Audience")]
    public string Audience { get; set; }
}
