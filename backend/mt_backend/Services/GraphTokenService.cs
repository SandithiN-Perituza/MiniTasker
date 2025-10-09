using Microsoft.Identity.Web;
using System;

namespace mt_backend.Services
{
    public class GraphTokenService
    {
        private readonly ITokenAcquisition _tokenAcquisition;

        public GraphTokenService(ITokenAcquisition tokenAcquisition)
        {
            _tokenAcquisition = tokenAcquisition;
        }

        public async Task<string> GetAccessTokenOnBehalfOfAsync(string[] scopes)
        {
            try
            {
                return await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token acquisition failed: {ex.Message}");
                throw;
            }

        }
    }
}
