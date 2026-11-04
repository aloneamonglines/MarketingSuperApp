using MKS.Identity.Domain.Shared.Enums;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Users.Entities
{
    /// <summary>
    /// Represents a global person identity; sign-in identifiers and credentials are stored separately.
    /// </summary>
    [EntityScope(CEntityScope.Global)]
    public sealed class UserEntity : FullAuditEntity
    {
        private readonly List<UserIdentifierEntity> _identifiers = [];

        private UserEntity()
        {
        }

        /// <summary>Gets or sets the user's given name.</summary>
        public string? FirstName { get; set; }
        /// <summary>Gets or sets the user's family name.</summary>
        public string? LastName { get; set; }
        /// <summary>Gets or sets the user's thumbnail URL.</summary>
        public string? Thumbnail { get; set; }
        /// <summary>Gets or sets the account status.</summary>
        public CMasterStatus MasterStatus { get; set; } = CMasterStatus.Active;
        /// <summary>Gets or sets the membership version used to invalidate authorization caches.</summary>
        public long MembershipVersion { get; set; }
        /// <summary>Gets all login identifiers owned by this user.</summary>
        public IReadOnlyCollection<UserIdentifierEntity> Identifiers => _identifiers;

        /// <summary>Creates a user after validating that at least one sign-in identifier exists.</summary>
        public static UserEntity Create(
            string? email,
            string? phoneNumber,
            string? username,
            string? firstName = null,
            string? lastName = null)
        {
            var user = new UserEntity { FirstName = firstName?.Trim(), LastName = lastName?.Trim() };
            var identifiers = UserIdentifierEntity.CreateSet(email, phoneNumber, username);
            foreach (var identifier in identifiers)
            {
                identifier.AttachTo(user.Id);
            }

            user._identifiers.AddRange(identifiers);
            return user;
        }

        /// <summary>Increments the authorization version after a membership or role change.</summary>
        public void IncrementMembershipVersion() => MembershipVersion++;
    }
}
