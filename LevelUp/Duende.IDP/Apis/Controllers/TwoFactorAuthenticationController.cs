using Duende.IDP.Models;
using Duende.IDP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Duende.IDP.Apis.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TwoFactorAuthenticationController : ControllerBase
{
    private readonly ITwoFactorAuthService _authService;

    public TwoFactorAuthenticationController(ITwoFactorAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<string> GetAuthenticatorKey(CancellationToken cancellationToken)
    {
        return await _authService.GetKey(cancellationToken);
    }


    [HttpGet("settings2fa")]
    public async Task<ActionResult<ServiceResponse<TwoFactorAuthSettings>>> GetSettings(CancellationToken cancellationToken)
    {
        var result = await _authService.GetSettings(cancellationToken);
        return result;
    }

    [HttpPut("enable/{code}")]
    public async Task<ActionResult<ServiceResponse<TwoFactorAuthSettings>>> Enable2FA(string code, CancellationToken cancellationToken)
    {
        var result = await _authService.Enable2FA(code, cancellationToken);
        return result;
    }

    [HttpPut("disable/{code}")]
    public async Task<ActionResult<ServiceResponse<TwoFactorAuthSettings>>> Disable2FA(string code, CancellationToken cancellationToken)
    {
        var result = await _authService.Disable2FA(code, cancellationToken);
        return result;
    }
}