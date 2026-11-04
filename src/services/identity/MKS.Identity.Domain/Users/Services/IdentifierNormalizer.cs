using MKS.Identity.Domain.Shared.Constants;
using System.Net.Mail;
using System.Text.RegularExpressions;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.Users.Services
{
    /// <summary>
    /// Normalizes and validates supported login identifiers.
    /// </summary>
    public static class IdentifierNormalizer
    {
        /// <summary>Normalizes an identifier for lookup and uniqueness checks.</summary>
        public static string Normalize(string value, CIdentifierType type)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            var trimmed = value.Trim();
            try
            {
                return type switch
                {
                    CIdentifierType.Email => NormalizeEmail(trimmed),
                    CIdentifierType.Phone => NormalizePhone(trimmed),
                    CIdentifierType.Username => NormalizeUsername(trimmed),
                    _ => throw new ArgumentOutOfRangeException(nameof(type))
                };
            }
            catch (Exception exception) when (exception is ArgumentException or FormatException)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidIdentifier, exception);
            }
        }

        private static string NormalizeEmail(string email)
        {
            var address = new MailAddress(email);
            if (!string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException(IdentityConstants.ValidationMessages.SingleEmailAddressRequired);
            }

            return email.ToLowerInvariant();
        }

        private static string NormalizePhone(string phone)
        {
            var normalized = Regex.Replace(phone, @"[\s().-]", string.Empty);
            if (!Regex.IsMatch(normalized, @"^\+[1-9]\d{7,14}$", RegexOptions.CultureInvariant))
            {
                throw new FormatException(IdentityConstants.ValidationMessages.PhoneMustUseE164);
            }

            return normalized;
        }

        private static string NormalizeUsername(string username)
        {
            var normalized = username.ToLowerInvariant();
            if (normalized.Length is < 3 or > 64 || normalized.Any(char.IsControl))
            {
                throw new FormatException(IdentityConstants.ValidationMessages.UsernameInvalid);
            }

            return normalized;
        }
    }
}
