using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Common;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Users.Entities
{
    /// <summary>
    /// Stores the password hash and sign-in lockout state for a user.
    /// </summary>
    [EntityScope(CEntityScope.Global)]
    public sealed class UserCredentialEntity : FullAuditEntity
    {
        private UserCredentialEntity()
        {
        }

        /// <summary>Gets or sets the owning user id.</summary>
        public Guid UserId { get; private set; }
        /// <summary>Gets or sets the password hash.</summary>
        public string PasswordHash { get; private set; } = string.Empty;
        /// <summary>Gets or sets whether the user must change a temporary password.</summary>
        public bool MustChangePassword { get; private set; }
        /// <summary>Gets or sets the number of consecutive failed sign-ins.</summary>
        public int FailedAttempts { get; private set; }
        /// <summary>Gets or sets when sign-in lockout ends.</summary>
        public DateTimeOffset? LockedUntil { get; private set; }
        /// <summary>Gets or sets when the password was last changed.</summary>
        public DateTimeOffset? PasswordChangedAt { get; private set; }

        /// <summary>Creates a credential record from an already hashed password.</summary>
        public static UserCredentialEntity Create(Guid userId, string passwordHash, bool mustChangePassword)
        {
            if (userId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
            return new UserCredentialEntity
            {
                UserId = userId,
                PasswordHash = passwordHash,
                MustChangePassword = mustChangePassword
            };
        }

        /// <summary>Changes the password hash and clears temporary-password and lockout state.</summary>
        public void ChangePassword(string passwordHash, DateTimeOffset now)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
            PasswordHash = passwordHash;
            MustChangePassword = false;
            FailedAttempts = 0;
            LockedUntil = null;
            PasswordChangedAt = now;
        }

        /// <summary>Records a failed sign-in and applies a temporary lockout at the threshold.</summary>
        public void RecordFailedAttempt(DateTimeOffset now, int threshold, TimeSpan lockoutDuration)
        {
            if (threshold < 1 || lockoutDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(threshold));
            }

            FailedAttempts++;
            if (FailedAttempts >= threshold)
            {
                LockedUntil = now.Add(lockoutDuration);
                FailedAttempts = 0;
            }
        }

        /// <summary>Clears failed sign-in state after a successful login.</summary>
        public void RecordSuccessfulSignIn()
        {
            FailedAttempts = 0;
            LockedUntil = null;
        }
    }
}
