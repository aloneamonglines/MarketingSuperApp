    namespace MKS.Shared.Application.Contracts.Apps.DTOs
{
    public record UpdateAppDto
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? LogoUrl { get; init; }
        public int MasterStatus { get; init; }
    }
}