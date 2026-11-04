using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Shared.Infrastructure.Persistence
{
    /// <summary>
    /// Holds the tenant scope established by an authorized application workflow.
    /// </summary>
    public sealed class TenantScopeAccessor : ITenantScopeAccessor
    {
        private readonly AsyncLocal<Guid?> _tenantId = new();

        /// <inheritdoc />
        public Guid? TenantId => _tenantId.Value;

        /// <inheritdoc />
        public IDisposable Enter(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("A tenant identifier is required.", nameof(tenantId));
            }

            var previousTenantId = _tenantId.Value;
            _tenantId.Value = tenantId;
            return new Scope(this, previousTenantId);
        }

        private sealed class Scope(TenantScopeAccessor accessor, Guid? previousTenantId) : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                accessor._tenantId.Value = previousTenantId;
                _disposed = true;
            }
        }
    }
}
