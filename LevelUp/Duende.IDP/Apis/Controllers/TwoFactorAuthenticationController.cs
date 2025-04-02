using Duende.IDP.Models;
using Duende.IDP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

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
    
    [HttpGet("api/download-qr-code")]
    public async Task<IActionResult> DownloadQrCode(CancellationToken cancellationToken)
    {
        var key = await _authService.GetKey(cancellationToken);
        
        // Generate the QR code image (this reuses your GenerateQr method logic)
        var qrCodeBytes = GenerateQrCodeBytes(key);

        // Return the image as a downloadable PNG file
        return File(qrCodeBytes, "image/png", "2fa-qr-code.png");
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
    
    private byte[] GenerateQrCodeBytes(string authenticatorKey)
    {
        if (string.IsNullOrEmpty(authenticatorKey))
        {
            return null;
        }

        var generator = new QRCodeGenerator();
        // Here, you can use the format you want for your QR code, e.g., "otpauth://totp/SimpleSprinkle?secret=..."
        var qrCodeFormat = $"otpauth://totp/LevelUp?secret={authenticatorKey}";

        // Create the QR code data
        var qrData = generator.CreateQrCode(qrCodeFormat, QRCodeGenerator.ECCLevel.L);
        BitmapByteQRCode code = new BitmapByteQRCode(qrData);

        // Get the graphic bytes (you can modify the scale factor, here it's 10)
        var codeBytes = code.GetGraphic(10);

        return codeBytes;
    }
}