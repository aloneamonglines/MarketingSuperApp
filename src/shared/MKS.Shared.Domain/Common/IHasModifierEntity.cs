namespace MKS.Shared.Domain.Common
{
    public interface IHasModifierEntity
    {
        Guid? ModifierId { get; set; }
        DateTimeOffset? ModifiedAt { get; set; }
    }
}