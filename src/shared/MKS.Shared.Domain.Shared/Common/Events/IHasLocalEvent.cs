namespace MKS.Shared.Domain.Shared.Common.Events
{
    public interface IHasLocalEvent
    {
        IReadOnlyCollection<object> LocalEvents { get; }
        void ClearLocalEvents();
    }
}