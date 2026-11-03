namespace MKS.Shared.Application.Contracts.Common.Interfaces
{
    public interface IHasRestorerDto : IBaseProfile
    {
        Guid? RestorerId { get; set; }
    }
}