namespace MKS.Shared.Application.Contracts.Common.DTOs
{
    /// <summary>Provides the common error envelope returned by APIs.</summary>
    public sealed record ApiResponse(
        bool Success,
        object? Data,
        ApiError? Error,
        string TraceId)
    {
        public static ApiResponse Failure(ApiError error, string traceId) =>
            new(false, null, error, traceId);
    }

    /// <summary>Describes a stable API error code, localized message, and optional field errors.</summary>
    public sealed record ApiError(
        string Code,
        string Message,
        IReadOnlyList<ApiErrorDetail> Details);

    /// <summary>Describes a validation error associated with a request field path.</summary>
    public sealed record ApiErrorDetail(
        string Field,
        string Code,
        string Message);
}
