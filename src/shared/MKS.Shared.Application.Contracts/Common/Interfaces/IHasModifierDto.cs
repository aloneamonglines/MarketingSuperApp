namespace MKS.Shared.Application.Contracts.Common.Interfaces
{
    public interface IHasModifierDto : IBaseProfile
    {
        Guid? ModifierId { get; set; }
    }
}