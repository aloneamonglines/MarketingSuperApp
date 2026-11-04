namespace MKS.Identity.Application.Contracts.Common.Interfaces
{
    /// <summary>
    /// Provides password hashing and policy validation without coupling application services to a provider.
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>Hashes a password for the specified user identifier.</summary>
        string Hash(Guid userId, string password);
        /// <summary>Verifies a password against the stored hash.</summary>
        bool Verify(Guid userId, string passwordHash, string password);
        /// <summary>Validates a user-selected password.</summary>
        bool MeetsPolicy(string password);
    }
}
