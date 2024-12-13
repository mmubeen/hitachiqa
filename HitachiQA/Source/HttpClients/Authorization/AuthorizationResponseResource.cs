using System.Text.Json.Serialization;

namespace HitachiQA.Source.HttpClients.Authorization;

public class AuthorizationResponseResource
{
    [JsonPropertyName("access_token"), Newtonsoft.Json.JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("token_type"), Newtonsoft.Json.JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in"), Newtonsoft.Json.JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope"), Newtonsoft.Json.JsonProperty("scope")]
    public string Scope { get; set; }

    [JsonPropertyName("refresh_token"), Newtonsoft.Json.JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("id_token"), Newtonsoft.Json.JsonProperty("id_token")]
    public string IdToken { get; set; }

    [JsonPropertyName("error"), Newtonsoft.Json.JsonProperty("error")]
    public string Error { get; set; }

    [JsonPropertyName("error_description"), Newtonsoft.Json.JsonProperty("error_description")]
    public string ErrorDescription { get; set; }

    [JsonPropertyName("error_codes"), Newtonsoft.Json.JsonProperty("error_codes")]
    public List<int> ErrorCodes { get; set; }

    [JsonPropertyName("timestamp"), Newtonsoft.Json.JsonProperty("timestamp")]
    public string Timestamp { get; set; }

    [JsonPropertyName("trace_id"), Newtonsoft.Json.JsonProperty("trace_id")]
    public string TraceId { get; set; }

    [JsonPropertyName("correlation_id"), Newtonsoft.Json.JsonProperty("correlation_id")]
    public string CorrelationId { get; set; }

    [Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public DateTime ExpiryDateTime { get; set; }

    public Credentials ToCredentials()
    => new()
    {
        AccessToken = AccessToken,
        ExpiryDateTime = ExpiryDateTime,
    };
}