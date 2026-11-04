using System.Security.Cryptography;
using MKS.Identity.Application.Contracts.Common.Interfaces;

namespace MKS.Identity.Infrastructure.Users
{
    /// <summary>
    /// Hashes passwords with versioned PBKDF2-SHA256 and constant-time verification.
    /// </summary>
    public sealed class Pbkdf2PasswordService : IPasswordService
    {
        private const int Iterations = 600_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        /// <inheritdoc />
        public string Hash(Guid userId, string password)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(password);
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);
            return $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <inheritdoc />
        public bool Verify(Guid userId, string passwordHash, string password)
        {
            ArgumentNullException.ThrowIfNull(passwordHash);
            ArgumentNullException.ThrowIfNull(password);
            var parts = passwordHash.Split('$');
            if (parts.Length != 4 ||
                parts[0] != "pbkdf2-sha256" ||
                !int.TryParse(parts[1], out var iterations) ||
                iterations is < 100_000 or > 2_000_000)
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expected = Convert.FromBase64String(parts[3]);
                if (salt.Length != SaltSize || expected.Length != HashSize)
                {
                    return false;
                }

                var actual = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool MeetsPolicy(string password) =>
            password.Length >= 12 &&
            password.Any(char.IsLower) &&
            password.Any(char.IsUpper) &&
            password.Any(char.IsDigit) &&
            password.Any(character => !char.IsLetterOrDigit(character));
    }
}
