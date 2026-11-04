namespace MKS.Shared.Application.Contracts.Common.DTOs
{
    /// <summary>Represents a validation failure mapped to a request field path.</summary>
    public sealed record ApiValidationFailure(string Field, string Code, string Message);
}
