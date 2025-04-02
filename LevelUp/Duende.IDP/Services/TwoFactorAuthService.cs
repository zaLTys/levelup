using System.Security.Claims;
using Duende.IDP.Models;
using Microsoft.AspNetCore.Identity;

namespace Duende.IDP.Services
{
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public TwoFactorAuthService(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<string> GetKey(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst("sub").Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }
            else
            {
                var isEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                if (isEnabled)
                    return null;

                await _userManager.ResetAuthenticatorKeyAsync(user);
                return await _userManager.GetAuthenticatorKeyAsync(user);
            }
        }

        public async Task<ServiceResponse<TwoFactorAuthSettings>> GetSettings(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst("sub").Value;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ServiceResponse<TwoFactorAuthSettings>.Error("User not found");
            }
            else
            {
                var isEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                return ServiceResponse<TwoFactorAuthSettings>.FromData(new TwoFactorAuthSettings(isEnabled));
            }
        }

        public async Task<ServiceResponse<TwoFactorAuthSettings>> Enable2FA(string code, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst("sub").Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResponse<TwoFactorAuthSettings>.Error("User not found");

            bool isValidCode = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

            if (isValidCode)
            {
                var isEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                if (isEnabled)
                    return ServiceResponse<TwoFactorAuthSettings>.Error("Already enabled");

                await _userManager.SetTwoFactorEnabledAsync(user, true);
                var newResult = await _userManager.GetTwoFactorEnabledAsync(user);
                var result = ServiceResponse<TwoFactorAuthSettings>.FromData(new TwoFactorAuthSettings(newResult));
                return result;
            }

            return ServiceResponse<TwoFactorAuthSettings>.Error("Invalid code");
        }

        public async Task<ServiceResponse<TwoFactorAuthSettings>> Disable2FA(string code, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ServiceResponse<TwoFactorAuthSettings>.Error("User not found");

            bool isValidCode = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

            if (isValidCode)
            {
                var isEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                if (!isEnabled)
                    return ServiceResponse<TwoFactorAuthSettings>.Error("Already disabled");
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                await _userManager.ResetAuthenticatorKeyAsync(user);

                var newResult = await _userManager.GetTwoFactorEnabledAsync(user);
                var result = ServiceResponse<TwoFactorAuthSettings>.FromData(new TwoFactorAuthSettings(newResult));
                return result;
            }

            return ServiceResponse<TwoFactorAuthSettings>.Error("Invalid code");
        }

    }
}
