using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Apps.Entities
{
    /// <summary>
    /// Grants application administrator rights to a user within one application.
    /// </summary>
    [EntityScope(CEntityScope.App)]
    public sealed class AppAdminAssignmentEntity : FullAuditEntity, IHasAppEntity
    {
        /// <summary>Gets or sets the application identifier.</summary>
        public Guid AppId { get; set; }
        /// <summary>Gets or sets the user identifier.</summary>
        public Guid UserId { get; set; }
        /// <summary>Gets or sets whether the assignment is active.</summary>
        public bool IsActive { get; set; } = true;
    }
}
