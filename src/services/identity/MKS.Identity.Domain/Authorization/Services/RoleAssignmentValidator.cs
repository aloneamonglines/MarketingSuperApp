using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.Authorization.Services
{
    /// <summary>
    /// Enforces application and tenant scope before persisting a role assignment.
    /// </summary>
    public static class RoleAssignmentValidator
    {
        /// <summary>Validates that role, membership, and assignment share the same app and tenant.</summary>
        public static void Validate(
            Guid roleAppId,
            Guid? roleTenantId,
            Guid membershipAppId,
            Guid membershipTenantId,
            Guid assignmentAppId,
            Guid assignmentTenantId,
            bool isAppAdmin)
        {
            if (roleAppId != membershipAppId
                || roleAppId != assignmentAppId
                || membershipTenantId != assignmentTenantId
                || roleTenantId is not null && roleTenantId != assignmentTenantId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            if (!isAppAdmin && roleTenantId is null)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }
        }
    }
}
