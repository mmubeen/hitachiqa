using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace HitachiQA.Helpers
{
    [Obsolete("Please use secrets from Configuration object")]
    public class KeyVault
    {
        private readonly string KEY_VAULT_URI;
        public KeyVault(string KEY_VAULT_URI)
        {
            this.KEY_VAULT_URI = KEY_VAULT_URI;
        }
        public string GetSecret(string secretName, bool optional)
        {
            KeyVaultSecret theSecret;
            string value;
            if (string.IsNullOrWhiteSpace(KEY_VAULT_URI))
            {
                Functions.HandleFailure(new ArgumentNullException("Helpers.KeyVault - KEY_VAULT_URI was not set properly"));
            }

            try
            {
                var secretBundle = new SecretClient(new Uri(Environment.GetEnvironmentVariable("APP_KEYVAULT_URI")), new DefaultAzureCredential());
                theSecret = secretBundle.GetSecret(secretName);

                value = theSecret.Name;
            }
            catch (Exception ex)
            {
                if (optional)
                {
                    return null;
                }
                else
                {
                    value = Functions.HandleFailure("Error while retrieving secrets from azure KeyVault", ex).ToString();
                }
            }
            return value;
        }

        public string GetSecret(string secretName)
        {
            return GetSecret(secretName, false);
        }
    }
}
