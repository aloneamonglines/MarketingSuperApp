namespace MKS.Shared.Application.Contracts.Apps.DTOs
{
    public record AppDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Code { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string? LogoUrl { get; init; }
        public int MasterStatus { get; init; }
    }
}