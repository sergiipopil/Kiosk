using System;
using System.Security.Cryptography;
using System.Text;

namespace KioskBrains.Server.Domain.Security
{
    public static class PasswordHelper
    {
        // todo: check https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing
        public static string GetPasswordHash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return null;
            }

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}