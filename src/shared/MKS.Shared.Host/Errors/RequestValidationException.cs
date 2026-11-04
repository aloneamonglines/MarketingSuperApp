using MKS.Shared.Application.Contracts.Common.DTOs;

namespace MKS.Shared.Host.Errors
{
    /// <summary>Represents request validation errors that should be returned to the caller.</summary>
    public sealed class RequestValidationException(
        IReadOnlyList<ApiValidationFailure> failures) : Exception("The request failed validation.")
    {
        public IReadOnlyList<ApiValidationFailure> Failures { get; } =
            failures is { Count: > 0 }
                ? failures
                : throw new ArgumentException("At least one validation failure is required.", nameof(failures));
    }
}
