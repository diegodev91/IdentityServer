using System;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace MVC.Services
{
    public interface ITokenService
    {
        Task<TokenResponse> GetToken(string scope);
    }
}
