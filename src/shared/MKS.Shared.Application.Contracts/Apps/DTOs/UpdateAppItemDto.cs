namespace MKS.Shared.Application.Contracts.Apps.DTOs
{
    /// <summary>Defines an application update in an atomic bulk request.</summary>
    public sealed record UpdateAppItemDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? LogoUrl { get; init; }
        public int MasterStatus { get; init; }
    }
}
