using MKS.Identity.Domain.Authorization.Repositories;
using MKS.Identity.Domain.Authorization.Services;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.Tests;

public sealed class PermissionEvaluatorTests
{
    [Fact]
    public async Task SuperAdminPlatformPermissionDoesNotGrantTenantDataAccess()
    {
        var repository = new StubAuthorizationRepository
        {
            PlatformPermissions = new HashSet<string>(["platform:SuperAdmin:*"], StringComparer.Ordinal)
        };
        var evaluator = new PermissionEvaluator(repository);

        var allowed = await evaluator.HasPermissionAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "marketing:campaigns:read");

        Assert.False(allowed);
    }

    [Fact]
    public async Task SuperAdminCanUsePlatformPermissionsWithoutTenantScope()
    {
        var repository = new StubAuthorizationRepository
        {
            PlatformPermissions = new HashSet<string>(["platform:SuperAdmin:*"], StringComparer.Ordinal)
        };
        var evaluator = new PermissionEvaluator(repository);

        var allowed = await evaluator.HasPermissionAsync(
            Guid.NewGuid(),
            Guid.Empty,
            null,
            "platform:apps:create");

        Assert.True(allowed);
    }

    [Fact]
    public async Task AppAdminHasAppPermissionsWithoutTenantMembership()
    {
        var repository = new StubAuthorizationRepository { IsAppAdmin = true };
        var evaluator = new PermissionEvaluator(repository);

        var allowed = await evaluator.HasPermissionAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "marketing:campaigns:delete");

        Assert.True(allowed);
    }

    [Fact]
    public async Task TenantAdminIsLimitedToTenantAdministrationPermissions()
    {
        var repository = new StubAuthorizationRepository
        {
            IsTenantAdmin = true,
            Membership = new TenantMembershipEntity { AppId = Guid.NewGuid(), TenantId = Guid.NewGuid() }
        };
        var evaluator = new PermissionEvaluator(repository);
        var userId = Guid.NewGuid();
        var appId = repository.Membership.AppId;
        var tenantId = repository.Membership.TenantId;

        Assert.True(await evaluator.HasPermissionAsync(
            userId,
            appId,
            tenantId,
            "tenant:admin:users:create"));
        Assert.False(await evaluator.HasPermissionAsync(
            userId,
            appId,
            tenantId,
            "marketing:campaigns:read"));
    }

    [Fact]
    public async Task TenantPermissionsRequireMatchingAppMembership()
    {
        var repository = new StubAuthorizationRepository
        {
            Membership = new TenantMembershipEntity { AppId = Guid.NewGuid(), TenantId = Guid.NewGuid() },
            RolePermissions = new HashSet<string>(["marketing:campaigns:read"], StringComparer.Ordinal)
        };
        var evaluator = new PermissionEvaluator(repository);

        var allowed = await evaluator.HasPermissionAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            repository.Membership.TenantId,
            "marketing:campaigns:read");

        Assert.False(allowed);
    }

    [Fact]
    public void RoleAssignmentValidatorRejectsCrossAppAssignments()
    {
        var exception = Assert.Throws<DomainException>(() => RoleAssignmentValidator.Validate(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            isAppAdmin: true));

        Assert.Equal("Identity.Errors.InvalidScope", exception.LocalizationKey);
    }

    private sealed class StubAuthorizationRepository : IAuthorizationRepository
    {
        public bool IsAppAdmin { get; init; }
        public bool IsTenantAdmin { get; init; }
        public TenantMembershipEntity? Membership { get; init; }
        public IReadOnlySet<string> PlatformPermissions { get; init; } = new HashSet<string>();
        public IReadOnlySet<string> RolePermissions { get; init; } = new HashSet<string>();

        public Task<bool> IsAppAdminAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default) =>
            Task.FromResult(IsAppAdmin);

        public Task<bool> IsTenantAdminAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default) =>
            Task.FromResult(IsTenantAdmin);

        public Task<IReadOnlySet<string>> GetRolePermissionsAsync(
            Guid userId,
            Guid appId,
            Guid tenantId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(RolePermissions);

        public Task<TenantMembershipEntity?> GetMembershipAsync(
            Guid userId,
            Guid tenantId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Membership);

        public Task<IReadOnlySet<string>> GetPlatformPermissionsAsync(
            Guid userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(PlatformPermissions);
    }
}
