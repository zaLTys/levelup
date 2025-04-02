namespace Duende.IDP.Models;

public class MFALoginRequest
{
    public string Username { get; set; }
    public string Code { get; set; }
}