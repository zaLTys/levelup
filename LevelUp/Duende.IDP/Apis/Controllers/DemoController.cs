using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Duende.IDP.Models;

public class DemoController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    // Constructor
    public DemoController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // Simulate a 2FA Login attempt via API
    [HttpPost("api/simulate-login")]
    public async Task<IActionResult> SimulateLogin([FromBody] MFALoginRequest model)
    {
        // Validate inputs
        if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Code))
        {
            return BadRequest("Username and Code must be provided.");
        }

        // Try to find the user by username
        var user = await _userManager.FindByNameAsync(model.Username);
        if (user == null)
        {
            return Unauthorized("Invalid username or password.");
        }

        // Here we simulate the 2FA validation logic
        var result2FA = await _signInManager.TwoFactorAuthenticatorSignInAsync(model.Code, true, false);

        if (!result2FA.Succeeded)
        {
            return Unauthorized("Invalid 2FA code.");
        }
        
        // Returning a success response
        return Ok("Login successful");
    }
}
