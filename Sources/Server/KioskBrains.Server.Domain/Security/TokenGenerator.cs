using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace KioskBrains.Server.Domain.Security
{
    public class TokenGenerator
    {
        public string Generate(CurrentUser currentUser)
        {
            var identity = new IdentityIssuer().Issue(currentUser);

            var now = DateTime.UtcNow;

            var expirationPeriod = currentUser.IsAppClient
                ? TimeSpan.FromDays(JwtAuthOptions.AppClientLifetimeDays)
                : TimeSpan.FromMinutes(JwtAuthOptions.PortalUserLifetimeMinutes);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: JwtAuthOptions.Issuer,
                audience: JwtAuthOptions.Audience,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(expirationPeriod),
                signingCredentials: new SigningCredentials(JwtAuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler()
                .WriteToken(jwtSecurityToken);

            return encodedJwt;
        }
    }
}