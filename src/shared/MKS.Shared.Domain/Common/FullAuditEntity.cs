namespace MKS.Shared.Domain.Common
{
    /// <summary>
    /// Represents an entity that has full audit information, including creation, modification, deletion, and restoration details.
    /// </summary>
    public abstract class FullAuditEntity
        : BaseEntity,
        IHasCreatorEntity,
        IHasModifierEntity,
        IHasDeleterEntity,
        IHasRestorerEntity
    {
        /// <summary>
        /// The unique identifier of the user who created the entity.
        /// </summary>
        public virtual Guid? CreatorId { get; set; }
        /// <summary>
        /// The date and time when the entity was created.
        /// </summary>
        public virtual DateTimeOffset CreatedAt { get; set; }
        /// <summary>
        /// The unique identifier of the user who last modified the entity.
        /// </summary>
        public virtual Guid? ModifierId { get; set; }
        /// <summary>
        /// The date and time when the entity was last modified.
        /// </summary>
        public virtual DateTimeOffset? ModifiedAt { get; set; }
        /// <summary>
        /// The unique identifier of the user who deleted the entity, if applicable.
        /// </summary>
        public virtual Guid? DeleterId { get; set; }
        /// <summary>
        /// The date and time when the entity was deleted, if applicable.
        /// </summary>
        public virtual DateTimeOffset? DeletedAt { get; set; }
        /// <summary>
        /// Indicates whether the entity is deleted.
        /// </summary>
        public virtual bool IsDeleted { get; set; }
        /// <summary>
        /// The unique identifier of the user who restored the entity, if applicable.
        /// </summary>
        public virtual Guid? RestorerId { get; set; }
        /// <summary>
        /// The date and time when the entity was restored, if applicable.
        /// </summary>
        public virtual DateTimeOffset? RestoredAt { get; set; }
    }
}