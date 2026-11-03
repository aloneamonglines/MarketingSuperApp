namespace MKS.Shared.Application.Contracts.Common.Interfaces
{
    public interface IHasCreatorDto : IBaseProfile
    {
        Guid? CreatorId { get; set; }
    }
}