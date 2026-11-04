using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Shared.Domain.Shared.Attributes
{
    /// <summary>
    /// Declares the isolation scope of a persisted entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class EntityScopeAttribute(CEntityScope scope) : Attribute
    {
        /// <summary>
        /// Gets the declared isolation scope.
        /// </summary>
        public CEntityScope Scope { get; } = scope;
    }
}
