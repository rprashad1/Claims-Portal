using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ClaimsPortal.Utils
{
    // PBKDF2 hasher using KeyDerivation to avoid obsolete API usage
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            var derived = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, Iterations, KeySize);

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(derived) + ":" + Iterations.ToString();
        }

        public static bool Verify(string password, string stored)
        {
            try
            {
                var parts = stored.Split(':');
                if (parts.Length != 3) return false;
                var salt = Convert.FromBase64String(parts[0]);
                var key = Convert.FromBase64String(parts[1]);
                var iterations = int.Parse(parts[2]);

                var attempted = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, iterations, key.Length);
                return CryptographicOperations.FixedTimeEquals(attempted, key);
            }
            catch
            {
                return false;
            }
        }
    }
}
