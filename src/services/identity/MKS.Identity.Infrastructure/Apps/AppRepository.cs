using Microsoft.EntityFrameworkCore;
using MKS.Identity.Domain.Apps.Entities;
using MKS.Shared.Domain.Apps.Repositories;

namespace MKS.Identity.Infrastructure.Apps
{
    /// <summary>
    /// Persists application catalog records.
    /// </summary>
    public sealed class AppRepository(Persistence.IdentityDbContext dbContext) : IAppRepository<AppEntity>
    {
        /// <inheritdoc />
        public Task<AppEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
            dbContext.Apps.SingleOrDefaultAsync(app => app.Id == id, cancellationToken);

        /// <inheritdoc />
        public Task<bool> CodeExistsAsync(string normalizedCode, CancellationToken cancellationToken = default) =>
            dbContext.Apps.AnyAsync(app => app.Code == normalizedCode, cancellationToken);

        /// <inheritdoc />
        public Task<bool> AnyCodeExistsAsync(
            IReadOnlyCollection<string> normalizedCodes,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(normalizedCodes);
            if (normalizedCodes.Count == 0)
            {
                return Task.FromResult(false);
            }

            var codes = normalizedCodes.Distinct(StringComparer.Ordinal).ToArray();
            return dbContext.Apps.AnyAsync(app => codes.Contains(app.Code), cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(AppEntity app, CancellationToken cancellationToken = default) =>
            await dbContext.Apps.AddAsync(app, cancellationToken);

        /// <inheritdoc />
        public Task UpdateAsync(AppEntity app, CancellationToken cancellationToken = default)
        {
            dbContext.Apps.Update(app);
            return Task.CompletedTask;
        }
    }
}
