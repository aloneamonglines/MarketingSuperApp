namespace MKS.Shared.Domain.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class LocalizationResourceAttribute : Attribute
    {
        public string ResourceName { get; set; } = string.Empty;
    }
}