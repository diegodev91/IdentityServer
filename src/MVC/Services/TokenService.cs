using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MVC.Services
{
    public class TokenService: ITokenService
    {
        private readonly ILogger<TokenService> logger;
        private readonly IOptions<IdentityServerSettings> identityServerSettings;
        private readonly DiscoveryDocumentResponse discoveryDocumentResponse;

        public TokenService(ILogger<TokenService> logger, 
        IOptions<IdentityServerSettings> identityServerSettings)
        {
            this.logger = logger;
            this.identityServerSettings = identityServerSettings;
            using var client = new HttpClient();
            this.discoveryDocumentResponse = client.GetDiscoveryDocumentAsync(identityServerSettings.Value.DiscoveryUrl).Result;
        }
        public async Task<TokenResponse> GetToken(string scope)
        {
            using var client = new HttpClient();
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest {
                    Address = discoveryDocumentResponse.TokenEndpoint, 
                    ClientId = identityServerSettings.Value.ClientId,
                    ClientSecret = identityServerSettings.Value.ClientSecret
                }
            );

            if (tokenResponse.IsError)
            {
                this.logger.LogError("error");
                throw new Exception();
            }

            return tokenResponse;
        }
    }
}
