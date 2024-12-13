namespace HitachiQA.Source.HttpClients.Authorization;

public class Credentials
{
    public string AccessToken { get; set; }
    public string ClientId { get; set; }
    public string Target { get; set; }
    public DateTime? ExpiryDateTime { get; set; }
}
