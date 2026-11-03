namespace MKS.Shared.Application.Contracts.Apps.DTOs
{
    public abstract record CreateAppDto
    {
        public virtual string? Name { get; set; } = null;
        public virtual string? Code { get; set; } = null;
        public virtual string? Description { get; set; } = null;
        public string? LogoUrl { get; set; } = null;
    }
}