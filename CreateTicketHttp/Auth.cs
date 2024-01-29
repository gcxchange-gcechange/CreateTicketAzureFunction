using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace CreateTicketHttp
{
    internal class Auth
    {
        public static string GetAPIKey(ILogger log)
        {
            log.LogInformation("graphAuth received a request.");

            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables().Build();

            string apiKey = string.Empty;
            string keyVaultUrl = config["keyVaultUrl"];
            string secretName = config["secretName"];

            try {
                SecretClientOptions optionsSecret = new SecretClientOptions()
                {
                    Retry =
                    {
                        Delay= TimeSpan.FromSeconds(2),
                        MaxDelay = TimeSpan.FromSeconds(16),
                        MaxRetries = 5,
                        Mode = RetryMode.Exponential
                    }
                };

                var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential(), optionsSecret);
                KeyVaultSecret secret = client.GetSecret(secretName);
                apiKey = secret.Value;
            }
            catch (Exception e) {
                log.LogError($"Message: {e.Message}");
                if (e.InnerException is not null)
                    log.LogError($"InnerException: {e.InnerException.Message}");
            }

            log.LogInformation("graphAuth processed a request.");

            return apiKey;
        }
    }
}