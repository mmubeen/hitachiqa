using System;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace HitachiQA.Helpers
{
    [Obsolete("Please use secrets from Configuration object")]
    public class KeyVault
    {
        private readonly String KEY_VAULT_URI;
        public KeyVault(String KEY_VAULT_URI)
        {
            this.KEY_VAULT_URI = KEY_VAULT_URI;
        }
        public String GetSecret(string secretName, bool optional)
        {
            KeyVaultSecret theSecret;
            String value;
            if(String.IsNullOrWhiteSpace(KEY_VAULT_URI))
            {
                Functions.handleFailure(new ArgumentNullException("Helpers.KeyVault - KEY_VAULT_URI was not set properly"));
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
                        value = Functions.handleFailure("Error while retrieving secrets from azure KeyVault", ex).ToString();
                    }
                }
            return value;
        }

        public String GetSecret(string secretName)
        {
            return GetSecret(secretName, false);
        } 
    }
}
