using MKS.Shared.Domain.Shared.Common.ValueObjects;

namespace MKS.Shared.Application.Contracts.Common.Interfaces
{
    /// <summary>
    /// Represents the base profile information for a user, including personal details and contact information.
    /// This interface is intended to be implemented by DTOs that require user profile information.
    /// It provides properties for first name, last name, email, phone number, thumbnail image, a list of thumbnail images, and a list of addresses.
    /// Implementing this interface allows for consistent handling of user profile data across different parts of the application.
    /// </summary>
    public interface IBaseProfile
    {
        /// <summary>
        /// The first name of the user.
        /// </summary>
        string? FirstName { get; set; }
        /// <summary>
        /// The last name of the user.
        /// </summary>
        string? LastName { get; set; }
        /// <summary>
        /// The email address of the user.
        /// </summary>
        string? Email { get; set; }
        /// <summary>
        /// The phone number of the user.
        /// </summary>
        string? PhoneNumber { get; set; }
        /// <summary>
        /// The thumbnail image of the user.
        /// </summary>
        string? Thumbnail { get; set; }
        /// <summary>
        /// The list of thumbnail images of the user.
        /// </summary>
        IReadOnlyList<string>? Thumbnails { get; set; }
        /// <summary>
        /// The list of addresses of the user.
        /// </summary>
        IReadOnlyList<Address>? Addresses { get; set; }
    }
}