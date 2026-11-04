using FluentValidation;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Domain.Shared.Constants;

namespace MKS.Shared.Application.Contracts.Common.Validation
{
    public sealed class PagedAndSortedResultRequestValidator
        : AbstractValidator<PagedAndSortedResultRequestDto>
    {
        public PagedAndSortedResultRequestValidator()
        {
            RuleFor(request => request.SkipCount)
                .GreaterThanOrEqualTo(0)
                .WithErrorCode(SharedValidationErrorCodes.OutOfRange)
                .WithMessage(SharedValidationErrorCodes.OutOfRange);
            RuleFor(request => request.MaxResultCount)
                .InclusiveBetween(1, PagingConstants.MaxMaxResultCount)
                .WithErrorCode(SharedValidationErrorCodes.OutOfRange)
                .WithMessage(SharedValidationErrorCodes.OutOfRange);
            RuleFor(request => request.Sorting)
                .MaximumLength(PagingConstants.MaxSortingLength)
                .WithErrorCode(SharedValidationErrorCodes.TooLong)
                .WithMessage(SharedValidationErrorCodes.TooLong);
            RuleFor(request => request.Filter)
                .MaximumLength(PagingConstants.MaxFilterLength)
                .WithErrorCode(SharedValidationErrorCodes.TooLong)
                .WithMessage(SharedValidationErrorCodes.TooLong);
        }
    }
}
