using MKS.Identity.Domain.Shared.Constants;
using System.Net.Mail;
using System.Text.RegularExpressions;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Users.Entities
{
    /// <summary>
    /// Represents a globally unique, normalized login identifier.
    /// </summary>
    [EntityScope(CEntityScope.Global)]
    public sealed class UserIdentifierEntity : FullAuditEntity
    {
        private UserIdentifierEntity()
        {
        }

        /// <summary>Gets or sets the owning user id.</summary>
        public Guid UserId { get; private set; }
        /// <summary>Gets or sets the identifier kind.</summary>
        public CIdentifierType Type { get; private set; }
        /// <summary>Gets or sets the display value.</summary>
        public string Value { get; private set; } = string.Empty;
        /// <summary>Gets or sets the canonical comparison value.</summary>
        public string NormalizedValue { get; private set; } = string.Empty;
        /// <summary>Gets or sets whether the identifier was derived during user creation.</summary>
        public bool IsDerived { get; private set; }
        /// <summary>Gets or sets when the identifier was verified.</summary>
        public DateTimeOffset? VerifiedAt { get; private set; }

        internal void AttachTo(Guid userId) => UserId = userId;

        internal static IReadOnlyList<UserIdentifierEntity> CreateSet(string? email, string? phone, string? username)
        {
            var candidates = new List<(CIdentifierType Type, string Value, bool IsDerived)>();
            if (!string.IsNullOrWhiteSpace(email))
            {
                candidates.Add((CIdentifierType.Email, email, false));
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                candidates.Add((CIdentifierType.Phone, phone, false));
            }

            var resolvedUsername = string.IsNullOrWhiteSpace(username)
                ? email ?? phone
                : username;
            if (string.IsNullOrWhiteSpace(resolvedUsername))
            {
                throw new DomainException(IdentityDomainErrorCodes.IdentifierRequired);
            }

            var isDerived = string.IsNullOrWhiteSpace(username);
            var normalizedUsername = Normalize(resolvedUsername, CIdentifierType.Username);
            var duplicateIdentifier = candidates.FindIndex(
                candidate => Normalize(candidate.Value, candidate.Type) == normalizedUsername);
            if (duplicateIdentifier >= 0 && isDerived)
            {
                var existing = candidates[duplicateIdentifier];
                candidates[duplicateIdentifier] = (existing.Type, existing.Value, true);
            }
            else if (duplicateIdentifier < 0)
            {
                candidates.Add((CIdentifierType.Username, resolvedUsername, isDerived));
            }

            return candidates
                .Select(candidate => Create(candidate.Type, candidate.Value, candidate.IsDerived))
                .DistinctBy(identifier => identifier.NormalizedValue, StringComparer.Ordinal)
                .ToArray();
        }

        private static UserIdentifierEntity Create(CIdentifierType type, string value, bool isDerived)
        {
            var normalized = Normalize(value, type);
            return new UserIdentifierEntity
            {
                Type = type,
                Value = value.Trim(),
                NormalizedValue = normalized,
                IsDerived = isDerived
            };
        }

        private static string Normalize(string value, CIdentifierType type)
        {
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
            try
            {
                var address = new MailAddress(email);
                if (!string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
                {
                    throw new FormatException();
                }

                return email.ToLowerInvariant();
            }
            catch (FormatException exception)
            {
                throw new ArgumentException(
                    IdentityConstants.ValidationMessages.EmailInvalid,
                    nameof(email),
                    exception);
            }
        }

        private static string NormalizePhone(string phone)
        {
            var normalized = Regex.Replace(phone, @"[\s().-]", string.Empty);
            if (!Regex.IsMatch(normalized, @"^\+[1-9]\d{7,14}$", RegexOptions.CultureInvariant))
            {
                throw new ArgumentException(
                    IdentityConstants.ValidationMessages.PhoneMustBeE164,
                    nameof(phone));
            }

            return normalized;
        }

        private static string NormalizeUsername(string username)
        {
            var normalized = username.ToLowerInvariant();
            if (normalized.Length is < 3 or > 64 || normalized.Any(char.IsControl))
            {
                throw new ArgumentException(
                    IdentityConstants.ValidationMessages.UsernameLengthInvalid,
                    nameof(username));
            }

            return normalized;
        }
    }
}
