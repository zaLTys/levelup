namespace Duende.IDP.Models
{
    public class TwoFactorAuthSettings
    {
        public bool TwoFactorAuthEnabled { get; set; }

        public TwoFactorAuthSettings(bool enabled)
        {
            TwoFactorAuthEnabled = enabled;
        }
        public TwoFactorAuthSettings()
        {

        }
    }
}
