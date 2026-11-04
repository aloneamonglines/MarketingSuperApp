namespace MKS.Identity.Application.Contracts.Common.Interfaces
{
    /// <summary>
    /// Coordinates atomic persistence of an Identity workflow.
    /// </summary>
    public interface IIdentityUnitOfWork
    {
        /// <summary>Runs an operation and commits all changes atomically.</summary>
        Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
    }
}
