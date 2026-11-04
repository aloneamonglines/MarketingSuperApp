using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Shared.Infrastructure.Persistence
{
    /// <summary>
    /// Sets the tenant setting transaction-locally for PostgreSQL row-level security policies.
    /// </summary>
    public sealed class TenantRlsTransactionInterceptor(
        ICurrentContext currentContext,
        MKS.Shared.Application.Contracts.Common.Interfaces.ITenantScopeAccessor tenantScope,
        IOptions<SharedPersistenceOptions> options) : DbTransactionInterceptor
    {
        /// <inheritdoc />
        public override DbTransaction TransactionStarted(
            DbConnection connection,
            TransactionEndEventData eventData,
            DbTransaction result)
        {
            SetTenantSetting(connection, result);
            return result;
        }

        /// <inheritdoc />
        public override async ValueTask<DbTransaction> TransactionStartedAsync(
            DbConnection connection,
            TransactionEndEventData eventData,
            DbTransaction result,
            CancellationToken cancellationToken = default)
        {
            await SetTenantSettingAsync(connection, result, cancellationToken);
            return result;
        }

        private void SetTenantSetting(DbConnection connection, DbTransaction transaction)
        {
            using var command = CreateCommand(connection, transaction);
            command.ExecuteNonQuery();
        }

        private async Task SetTenantSettingAsync(
            DbConnection connection,
            DbTransaction transaction,
            CancellationToken cancellationToken)
        {
            await using var command = CreateCommand(connection, transaction);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        private DbCommand CreateCommand(DbConnection connection, DbTransaction transaction)
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT set_config(@setting_name, @tenant_id, true)";

            var settingParameter = command.CreateParameter();
            settingParameter.ParameterName = "setting_name";
            settingParameter.Value = options.Value.TenantSettingName;
            command.Parameters.Add(settingParameter);

            var tenantParameter = command.CreateParameter();
            tenantParameter.ParameterName = "tenant_id";
            tenantParameter.Value = (tenantScope.TenantId ?? currentContext.TenantId)?.ToString() ?? string.Empty;
            command.Parameters.Add(tenantParameter);

            return command;
        }
    }
}
