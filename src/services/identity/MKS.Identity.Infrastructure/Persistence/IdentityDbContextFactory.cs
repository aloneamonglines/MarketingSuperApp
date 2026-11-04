using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Identity.Infrastructure.Persistence
{
    /// <summary>
    /// Creates the Identity context for EF Core tooling without starting the web host.
    /// </summary>
    public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
    {
        /// <inheritdoc />
        public IdentityDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<IdentityDbContext>()
                .UseNpgsql("Host=localhost;Database=mks_identity;Username=mks_identity")
                .Options;
            return new IdentityDbContext(options, new DesignTimeCurrentContext(), new DesignTimeTenantScopeAccessor());
        }

        private sealed class DesignTimeCurrentContext : ICurrentContext
        {
            public Guid? UserId => null;
            public Guid? SessionId => null;
            public Guid? TenantId => null;
            public Guid? AppId => null;
            public Guid? ImpersonatorId => null;
            public Guid? BreakGlassGrantId => null;
        }

        private sealed class DesignTimeTenantScopeAccessor : ITenantScopeAccessor
        {
            public Guid? TenantId => null;

            public IDisposable Enter(Guid tenantId) =>
                throw new NotSupportedException(IdentityConstants.ConfigurationErrors.TenantScopeUnavailableAtDesignTime);
        }
    }
}
