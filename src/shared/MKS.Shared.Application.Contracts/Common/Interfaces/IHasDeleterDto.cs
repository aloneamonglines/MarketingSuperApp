namespace MKS.Shared.Application.Contracts.Common.Interfaces
{
    public interface IHasDeleterDto : IBaseProfile
    {
        Guid? DeleterId { get; set; }
    }
}