namespace MKS.Identity.Domain.Shared.Enums
{
    /// <summary>
    /// Identifies a supported user sign-in identifier.
    /// </summary>
    public enum CIdentifierType
    {
        /// <summary>An email address.</summary>
        Email = 1,
        /// <summary>An E.164 telephone number.</summary>
        Phone = 2,
        /// <summary>A username.</summary>
        Username = 3
    }
}
