using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Shared.Domain.Common
{
    public interface IHasMasterStatusEntity
    {
        CMasterStatus MasterStatus { get; set; }
    }
}