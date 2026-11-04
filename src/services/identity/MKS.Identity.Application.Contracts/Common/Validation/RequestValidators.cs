using System.Text.RegularExpressions;
using FluentValidation;
using MKS.Identity.Application.Contracts.Authorization.DTOs;
using MKS.Identity.Application.Contracts.MultiTenancy.DTOs;
using MKS.Identity.Application.Contracts.Platform.DTOs;
using MKS.Identity.Application.Contracts.Users.DTOs;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Enums;
using static MKS.Identity.Application.Contracts.Common.Validation.RequestValidationRules;

namespace MKS.Identity.Application.Contracts.Common.Validation
{
    public sealed class CreateTenantDtoValidator : AbstractValidator<CreateTenantDto>
    {
        public CreateTenantDtoValidator()
        {
            Required(request => request.Name);
            Required(request => request.Code);
            Required(request => request.AdminEmail);
            RuleFor(request => request.AdminEmail).EmailAddress()
                .When(request => !string.IsNullOrWhiteSpace(request.AdminEmail))
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
            RuleFor(request => request.AdminPhoneNumber)
                .Must(IsOptionalE164Phone)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
            RuleFor(request => request.AdminUsername)
                .Must(username => string.IsNullOrWhiteSpace(username) || username.Trim().Length is >= 3 and <= 64)
                .WithErrorCode(SharedValidationErrorCodes.OutOfRange)
                .WithMessage(SharedValidationErrorCodes.OutOfRange);
        }

        private void Required<T>(System.Linq.Expressions.Expression<Func<CreateTenantDto, T>> property) =>
            RuleFor(property).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);

        private static bool IsOptionalE164Phone(string? phone) =>
            string.IsNullOrWhiteSpace(phone) || Regex.IsMatch(phone.Trim(), @"^\+[1-9]\d{7,14}$");
    }

    public sealed class UpdateTenantDtoValidator : AbstractValidator<UpdateTenantDto>
    {
        public UpdateTenantDtoValidator()
        {
            RuleFor(request => request.Name).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.MasterStatus).Must(IsActiveStatus)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }

        private static bool IsActiveStatus(int status) =>
            status is (int)CMasterStatus.Active or (int)CMasterStatus.Inactive;
    }

    public sealed class UpdateTenantItemDtoValidator : AbstractValidator<UpdateTenantItemDto>
    {
        public UpdateTenantItemDtoValidator()
        {
            RuleFor(request => request.Id).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.Name).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.MasterStatus).Must(status =>
                    status is (int)CMasterStatus.Active or (int)CMasterStatus.Inactive)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    public sealed class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(request => request.TenantId).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.Email)
                .Must(email => string.IsNullOrWhiteSpace(email) || IsSingleEmail(email))
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
            RuleFor(request => request.PhoneNumber)
                .Must(phone => string.IsNullOrWhiteSpace(phone) || Regex.IsMatch(phone.Trim(), @"^\+[1-9]\d{7,14}$"))
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
            RuleFor(request => request.Username)
                .Must(username => string.IsNullOrWhiteSpace(username) || username.Trim().Length is >= 3 and <= 64)
                .WithErrorCode(SharedValidationErrorCodes.OutOfRange)
                .WithMessage(SharedValidationErrorCodes.OutOfRange);
            RuleFor(request => request.Email)
                .Must((request, _) =>
                    !string.IsNullOrWhiteSpace(request.Email) ||
                    !string.IsNullOrWhiteSpace(request.PhoneNumber) ||
                    !string.IsNullOrWhiteSpace(request.Username))
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
        }

        private static bool IsSingleEmail(string email) =>
            System.Net.Mail.MailAddress.TryCreate(email, out var address) &&
            string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
    }

    public sealed class CreateManyUsersDtoValidator : AbstractValidator<CreateManyUsersDto>
    {
        public CreateManyUsersDtoValidator()
        {
            RuleFor(request => request.Users)
                .NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.CollectionEmpty)
                .WithMessage(SharedValidationErrorCodes.CollectionEmpty);
            RuleFor(request => request.Users)
                .Must(users => users.Count <= PagingConstants.MaxMaxResultCount)
                .WithErrorCode(SharedValidationErrorCodes.CollectionTooLarge)
                .WithMessage(SharedValidationErrorCodes.CollectionTooLarge);
            RuleForEach(request => request.Users).SetValidator(new CreateUserDtoValidator());
        }
    }

    public sealed class GetManyUsersDtoValidator : AbstractValidator<GetManyUsersDto>
    {
        public GetManyUsersDtoValidator() => ValidateIds(request => request.UserIds);

        private void ValidateIds(System.Linq.Expressions.Expression<Func<GetManyUsersDto, IReadOnlyList<Guid>>> property) =>
            RuleFor(property)
                .Must(ValidIds)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
    }

    public sealed class DeactivateUsersDtoValidator : AbstractValidator<DeactivateUsersDto>
    {
        public DeactivateUsersDtoValidator()
        {
            RuleFor(request => request.UserIds).Must(ValidIds)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    public sealed class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(request => request.CurrentPassword).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.NewPassword).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
        }
    }

    public sealed class AuthenticateUserDtoValidator : AbstractValidator<AuthenticateUserDto>
    {
        public AuthenticateUserDtoValidator()
        {
            RuleFor(request => request.Identifier).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.Password).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.AppId).Must(id => id is null || id.Value != Guid.Empty)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
            RuleFor(request => request.TenantId).Must(id => id is null || id.Value != Guid.Empty)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    public sealed class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
    {
        public UpdateUserProfileDtoValidator()
        {
            RuleFor(request => request.FirstName).MaximumLength(128)
                .WithErrorCode(SharedValidationErrorCodes.TooLong)
                .WithMessage(SharedValidationErrorCodes.TooLong);
            RuleFor(request => request.LastName).MaximumLength(128)
                .WithErrorCode(SharedValidationErrorCodes.TooLong)
                .WithMessage(SharedValidationErrorCodes.TooLong);
        }
    }

    public sealed class UpdateUserProfileItemDtoValidator : AbstractValidator<UpdateUserProfileItemDto>
    {
        public UpdateUserProfileItemDtoValidator()
        {
            RuleFor(request => request.UserId).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.FirstName).MaximumLength(128)
                .WithErrorCode(SharedValidationErrorCodes.TooLong)
                .WithMessage(SharedValidationErrorCodes.TooLong);
            RuleFor(request => request.LastName).MaximumLength(128)
                .WithErrorCode(SharedValidationErrorCodes.TooLong)
                .WithMessage(SharedValidationErrorCodes.TooLong);
        }
    }

    public sealed class BulkUpdateUserProfilesDtoValidator : AbstractValidator<BulkUpdateUserProfilesDto>
    {
        public BulkUpdateUserProfilesDtoValidator()
        {
            RuleFor(request => request.Users).Must(ValidCollection)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
            RuleForEach(request => request.Users).SetValidator(new UpdateUserProfileItemDtoValidator());
        }
    }

    public sealed class UserPageRequestDtoValidator : AbstractValidator<UserPageRequestDto>
    {
        public UserPageRequestDtoValidator()
        {
            RuleFor(request => request.SkipCount).GreaterThanOrEqualTo(0)
                .WithErrorCode(SharedValidationErrorCodes.OutOfRange)
                .WithMessage(SharedValidationErrorCodes.OutOfRange);
            RuleFor(request => request.MaxResultCount).InclusiveBetween(1, PagingConstants.MaxMaxResultCount)
                .WithErrorCode(SharedValidationErrorCodes.OutOfRange)
                .WithMessage(SharedValidationErrorCodes.OutOfRange);
            RuleFor(request => request.Sorting).MaximumLength(PagingConstants.MaxSortingLength)
                .WithErrorCode(SharedValidationErrorCodes.TooLong)
                .WithMessage(SharedValidationErrorCodes.TooLong);
            RuleFor(request => request.Filter).MaximumLength(PagingConstants.MaxFilterLength)
                .WithErrorCode(SharedValidationErrorCodes.TooLong)
                .WithMessage(SharedValidationErrorCodes.TooLong);
            RuleFor(request => request.Status).Must(status => status is null || Enum.IsDefined(status.Value))
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    public sealed class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleDtoValidator()
        {
            RuleFor(request => request.Name).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.Code).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.FunctionCodes).Must(ValidStringCollection)
                .WithErrorCode(SharedValidationErrorCodes.CollectionEmpty)
                .WithMessage(SharedValidationErrorCodes.CollectionEmpty);
            RuleFor(request => request.PermissionCodes).Must(ValidStringCollection)
                .WithErrorCode(SharedValidationErrorCodes.CollectionEmpty)
                .WithMessage(SharedValidationErrorCodes.CollectionEmpty);
        }
    }

    public sealed class RoleIdsDtoValidator : AbstractValidator<RoleIdsDto>
    {
        public RoleIdsDtoValidator()
        {
            RuleFor(request => request.Ids).Must(ValidIds)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    public sealed class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
    {
        public UpdateRoleDtoValidator()
        {
            RuleFor(request => request.Name).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.MasterStatus).Must(IsActiveStatus)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    public sealed class UpdateRoleItemDtoValidator : AbstractValidator<UpdateRoleItemDto>
    {
        public UpdateRoleItemDtoValidator()
        {
            RuleFor(request => request.Id).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.Name).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.MasterStatus).Must(IsActiveStatus)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    public sealed class AdminAssignmentIdsDtoValidator : AbstractValidator<AdminAssignmentIdsDto>
    {
        public AdminAssignmentIdsDtoValidator()
        {
            RuleFor(request => request.UserIds).Must(ValidIds)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    public sealed class AdminAssignmentPagedRequestDtoValidator : AbstractValidator<AdminAssignmentPagedRequestDto>
    {
        public AdminAssignmentPagedRequestDtoValidator()
        {
            RuleFor(request => request.Filter)
                .Must(filter => string.IsNullOrWhiteSpace(filter) ||
                    (Guid.TryParse(filter, out var userId) && userId != Guid.Empty))
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    public sealed class CreateBreakGlassGrantDtoValidator : AbstractValidator<CreateBreakGlassGrantDto>
    {
        public CreateBreakGlassGrantDtoValidator()
        {
            RuleFor(request => request.TenantId).NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.Reason).MinimumLength(20)
                .WithErrorCode(SharedValidationErrorCodes.TooShort)
                .WithMessage(SharedValidationErrorCodes.TooShort);
            RuleFor(request => request.DurationMinutes).InclusiveBetween(1, 60)
                .WithErrorCode(SharedValidationErrorCodes.OutOfRange)
                .WithMessage(SharedValidationErrorCodes.OutOfRange);
        }
    }

    public sealed class BreakGlassGrantIdsDtoValidator : AbstractValidator<BreakGlassGrantIdsDto>
    {
        public BreakGlassGrantIdsDtoValidator()
        {
            RuleFor(request => request.Ids).Must(ValidIds)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    public sealed class BreakGlassGrantPageRequestDtoValidator : AbstractValidator<BreakGlassGrantPageRequestDto>
    {
        public BreakGlassGrantPageRequestDtoValidator()
        {
            RuleFor(request => request.SkipCount).GreaterThanOrEqualTo(0)
                .WithErrorCode(SharedValidationErrorCodes.OutOfRange)
                .WithMessage(SharedValidationErrorCodes.OutOfRange);
            RuleFor(request => request.MaxResultCount).InclusiveBetween(1, PagingConstants.MaxMaxResultCount)
                .WithErrorCode(SharedValidationErrorCodes.OutOfRange)
                .WithMessage(SharedValidationErrorCodes.OutOfRange);
            RuleFor(request => request.Sorting).MaximumLength(PagingConstants.MaxSortingLength)
                .WithErrorCode(SharedValidationErrorCodes.TooLong)
                .WithMessage(SharedValidationErrorCodes.TooLong);
            RuleFor(request => request.TenantId).Must(id => id is null || id.Value != Guid.Empty)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }

    internal static class RequestValidationRules
    {
        internal static bool ValidIds(IReadOnlyCollection<Guid>? ids) =>
            ids is { Count: > 0 and <= PagingConstants.MaxMaxResultCount } &&
            ids.All(id => id != Guid.Empty) &&
            ids.Distinct().Count() == ids.Count;

        internal static bool ValidCollection<T>(IReadOnlyCollection<T>? values) =>
            values is { Count: > 0 and <= PagingConstants.MaxMaxResultCount };

        internal static bool ValidStringCollection(IReadOnlyCollection<string>? values) =>
            ValidCollection(values) && values!.All(value => !string.IsNullOrWhiteSpace(value));

        internal static bool IsActiveStatus(int status) =>
            status is (int)CMasterStatus.Active or (int)CMasterStatus.Inactive;
    }
}
