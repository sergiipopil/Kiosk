using System.Collections.Generic;
using System.Security.Claims;

namespace KioskBrains.Server.Domain.Security
{
    public class IdentityIssuer
    {
        public ClaimsIdentity Issue(CurrentUser userInfo)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, userInfo.Id.ToString()),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, userInfo.Role.ToString()),
                    new Claim(AppClaimsIdentity.CustomerId, userInfo.CustomerId.ToString()),
                    new Claim(AppClaimsIdentity.TimeZoneName, userInfo.TimeZoneName ?? ""),
                };
            var identity = new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            return identity;
        }
    }
}