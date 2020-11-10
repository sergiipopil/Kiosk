using System;
using System.Text;

namespace KioskBrains.Server.Domain.Security
{
    public class KioskSecretKeyGenerator
    {
        private const string SecretKeySymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public const int KioskSerialKeyLength = 128;

        public const int IntegrationClientAccessKeyLength = 64;

        public string GenerateKioskSerialKey()
        {
            return GenerateSecretKey(KioskSerialKeyLength);
        }

        public string GenerateIntegrationClientAccessKey()
        {
            return GenerateSecretKey(IntegrationClientAccessKeyLength);
        }

        private string GenerateSecretKey(int length)
        {
            var random = new Random();
            var secretKeyBuilder = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                secretKeyBuilder.Append(SecretKeySymbols[random.Next(SecretKeySymbols.Length)]);
            }
            return secretKeyBuilder.ToString();
        }
    }
}