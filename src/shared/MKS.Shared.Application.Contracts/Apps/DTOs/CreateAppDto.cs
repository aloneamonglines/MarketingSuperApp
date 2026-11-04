namespace MKS.Shared.Application.Contracts.Apps.DTOs
{
    public record CreateAppDto
    {
        public string Name { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? LogoUrl { get; init; }
    }
}