using FluentValidation;
using MKS.Shared.Application.Contracts.Apps.DTOs;
using MKS.Shared.Domain.Shared.Constants;

namespace MKS.Shared.Application.Contracts.Apps.Validation
{
    public sealed class CreateAppDtoValidator : AbstractValidator<CreateAppDto>
    {
        public CreateAppDtoValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.Code)
                .NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
        }
    }

    public sealed class UpdateAppDtoValidator : AbstractValidator<UpdateAppDto>
    {
        public UpdateAppDtoValidator()
        {
            RuleFor(request => request.Name)
                .NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.MasterStatus)
                .Must(IsValidMasterStatus)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }

        private static bool IsValidMasterStatus(int status) => status is 1 or 2;
    }

    public sealed class UpdateAppItemDtoValidator : AbstractValidator<UpdateAppItemDto>
    {
        public UpdateAppItemDtoValidator()
        {
            RuleFor(request => request.Id)
                .NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.Name)
                .NotEmpty()
                .WithErrorCode(SharedValidationErrorCodes.Required)
                .WithMessage(SharedValidationErrorCodes.Required);
            RuleFor(request => request.MasterStatus)
                .Must(status => status is 1 or 2)
                .WithErrorCode(SharedValidationErrorCodes.Invalid)
                .WithMessage(SharedValidationErrorCodes.Invalid);
        }
    }
}
