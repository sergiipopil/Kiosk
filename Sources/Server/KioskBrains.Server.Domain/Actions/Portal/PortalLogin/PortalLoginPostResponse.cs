namespace KioskBrains.Server.Domain.Actions.Portal.PortalLogin
{
    public class PortalLoginPostResponse
    {
        public string Token { get; set; }

        public PortalUserInfo User { get; set; }
    }
}