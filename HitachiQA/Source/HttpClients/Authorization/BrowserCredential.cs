using System.Text.Json.Serialization;

namespace HitachiQA.Source.HttpClients.Authorization;

public class BrowserCredential
{

    [JsonPropertyName("homeAccountId"), Newtonsoft.Json.JsonProperty("homeAccountId")]
    public string HomeAccountId { get; set; }

    [JsonPropertyName("credentialType"), Newtonsoft.Json.JsonProperty("credentialType")]
    public string CredentialType { get; set; }

    [JsonPropertyName("secret"), Newtonsoft.Json.JsonProperty("secret")]
    public string Secret { get; set; }

    [JsonPropertyName("cachedAt"), Newtonsoft.Json.JsonProperty("cachedAt")]
    public string CachedAt { get; set; }

    [JsonPropertyName("expiresOn"), Newtonsoft.Json.JsonProperty("expiresOn")]
    public string ExpiresOn { get; set; }

    [JsonPropertyName("extendedExpiresOn"), Newtonsoft.Json.JsonProperty("extendedExpiresOn")]
    public string ExtendedExpiresOn { get; set; }

    [JsonPropertyName("environment"), Newtonsoft.Json.JsonProperty("environment")]
    public string Environment { get; set; }

    [JsonPropertyName("clientId"), Newtonsoft.Json.JsonProperty("clientId")]
    public string ClientId { get; set; }

    [JsonPropertyName("realm"), Newtonsoft.Json.JsonProperty("realm")]
    public string Realm { get; set; }

    [JsonPropertyName("target"), Newtonsoft.Json.JsonProperty("target")]
    public string Target { get; set; }

    [JsonPropertyName("tokenType"), Newtonsoft.Json.JsonProperty("tokenType")]
    public string TokenType { get; set; }

    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public DateTime ExpiryDateTime { get; set; }
}
