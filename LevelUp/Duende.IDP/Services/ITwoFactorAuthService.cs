using Duende.IDP.Models;

namespace Duende.IDP.Services
{
    public interface ITwoFactorAuthService
    {
        Task<string> GetKey(CancellationToken cancellationToken);
        Task<ServiceResponse<TwoFactorAuthSettings>> GetSettings(CancellationToken cancellationToken);
        Task<ServiceResponse<TwoFactorAuthSettings>> Enable2FA(string code, CancellationToken cancellationToken);
        Task<ServiceResponse<TwoFactorAuthSettings>> Disable2FA(string code, CancellationToken cancellationToken);
    }
}