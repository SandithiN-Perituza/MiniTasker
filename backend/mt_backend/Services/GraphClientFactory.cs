using Azure.Identity;
using Azure.Core;
using System.Threading.Tasks;
using DotNetEnv;
public static class GraphTokenProvider
{
    public static async Task<string> GetAccessTokenAsync()
    {
        Env.Load(); // Loads from .env file
        var clientId = Environment.GetEnvironmentVariable("AZURE_AD_CLIENT_SECRET"); 
        var tenantId = Environment.GetEnvironmentVariable("AZURE_AD_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("AZURE_AD_TENANT_ID");

        var credential = new ClientSecretCredential(
            tenantId, clientId, clientSecret,
            new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });

        var tokenRequestContext = new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" });

        var accessToken = await credential.GetTokenAsync(tokenRequestContext);

        return accessToken.Token;
    }
}
