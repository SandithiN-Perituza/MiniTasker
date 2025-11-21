using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using mt_backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace mt_backend.Services
{
    public class GraphTokenService : IGraphTokenService
    {
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _configuration;
        private readonly IErrorLogger _errorLogger;

        public GraphTokenService(ITokenAcquisition tokenAcquisition, IConfiguration configuration, IErrorLogger errorLogger)
        {
            _tokenAcquisition = tokenAcquisition;
            _configuration = configuration;
            _errorLogger = errorLogger;
        }

        // Legacy method (without user token parameter) - works with current HTTP context
        public async Task<string> GetAccessTokenOnBehalfOfAsync(string[] scopes)
        {
            try
            {
                return await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            }
            catch (Exception ex)
            {
                await _errorLogger.LogAsync(
                    $"Token acquisition failed: {ex.Message}",
                    ex.StackTrace ?? "No stack trace",
                    "GraphTokenService.GetAccessTokenOnBehalfOfAsync"
                );
                throw;
            }
        }

        // ✅ NEW: Manual MSAL.NET OBO implementation for when we have a specific user token
        public async Task<string> GetAccessTokenOnBehalfOfAsync(string[] scopes, string userAccessToken)
        {
            try
            {
                // ✅ Use MSAL.NET directly for OBO when we have a specific user token
                var clientId = _configuration["AzureAd:ClientId"];
                var clientSecret = _configuration["AzureAd:ClientSecret"];
                var tenantId = _configuration["AzureAd:TenantId"];

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(tenantId))
                {
                    var configError = "Azure AD configuration is missing. Ensure ClientId, ClientSecret, and TenantId are configured.";
                    await _errorLogger.LogAsync(
                        configError,
                        "Configuration validation failed",
                        "GraphTokenService.GetAccessTokenOnBehalfOfAsync"
                    );
                    throw new InvalidOperationException(configError);
                }

                var authority = $"https://login.microsoftonline.com/{tenantId}";

                var cca = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();

                var userAssertion = new UserAssertion(userAccessToken);
                var result = await cca.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();

                await _errorLogger.LogAsync(
                    "OBO token acquired successfully",
                    $"Audience: {result.Account?.Environment}, Scopes: {string.Join(", ", result.Scopes)}",
                    "GraphTokenService.GetAccessTokenOnBehalfOfAsync"
                );

                return result.AccessToken;
            }
            catch (Exception ex)
            {
                var errorMessage = $"OBO Token acquisition failed: {ex.Message}";
                var stackTrace = ex.StackTrace ?? "No stack trace";

                // Log more details for debugging
                if (ex is MsalServiceException msalEx)
                {
                    errorMessage += $" | MSAL Error Code: {msalEx.ErrorCode}";
                    stackTrace = $"MSAL Error Details: {msalEx.Message}\n{stackTrace}";
                }

                await _errorLogger.LogAsync(
                    errorMessage,
                    stackTrace,
                    "GraphTokenService.GetAccessTokenOnBehalfOfAsync"
                );

                throw;
            }
        }
    }
}