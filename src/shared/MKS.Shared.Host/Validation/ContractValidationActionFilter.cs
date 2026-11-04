using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Domain.Shared.Constants;
using MKS.Shared.Host.Errors;

namespace MKS.Shared.Host.Validation
{
    /// <summary>Runs FluentValidation validators for action contract arguments before controller execution.</summary>
    public sealed class ContractValidationActionFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var failures = new List<ApiValidationFailure>();
            foreach (var (argumentName, argument) in context.ActionArguments)
            {
                await ValidateArgumentAsync(
                    argument,
                    argumentName,
                    failures,
                    context.HttpContext.RequestAborted);
            }

            if (failures.Count > 0)
            {
                throw new RequestValidationException(failures);
            }

            await next();
        }

        private async Task ValidateArgumentAsync(
            object? argument,
            string fieldPrefix,
            ICollection<ApiValidationFailure> failures,
            CancellationToken cancellationToken)
        {
            if (argument is null || argument is string)
            {
                return;
            }

            if (argument is System.Collections.IEnumerable collection)
            {
                var items = collection.Cast<object?>().ToArray();
                if (items.Length == 0)
                {
                    failures.Add(new ApiValidationFailure(
                        fieldPrefix,
                        SharedValidationErrorCodes.CollectionEmpty,
                        SharedValidationErrorCodes.CollectionEmpty));
                }
                else if (items.Length > PagingConstants.MaxMaxResultCount)
                {
                    failures.Add(new ApiValidationFailure(
                        fieldPrefix,
                        SharedValidationErrorCodes.CollectionTooLarge,
                        SharedValidationErrorCodes.CollectionTooLarge));
                }

                if (items.OfType<Guid>().Any(id => id == Guid.Empty))
                {
                    failures.Add(new ApiValidationFailure(
                        fieldPrefix,
                        SharedValidationErrorCodes.Invalid,
                        SharedValidationErrorCodes.Invalid));
                }

                if (items.OfType<Guid>().Distinct().Count() != items.OfType<Guid>().Count())
                {
                    failures.Add(new ApiValidationFailure(
                        fieldPrefix,
                        SharedValidationErrorCodes.Invalid,
                        SharedValidationErrorCodes.Invalid));
                }

                for (var index = 0; index < items.Length; index++)
                {
                    await ValidateArgumentAsync(
                        items[index],
                        $"{fieldPrefix}[{index}]",
                        failures,
                        cancellationToken);
                }

                return;
            }

            var validators = serviceProvider.GetServices<IValidator>()
                .Where(validator => validator.CanValidateInstancesOfType(argument.GetType()));
            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(
                    new ValidationContext<object>(argument),
                    cancellationToken);
                foreach (var failure in result.Errors.Where(failure => failure is not null))
                {
                    failures.Add(new ApiValidationFailure(
                        JoinFieldPath(fieldPrefix, failure.PropertyName),
                        failure.ErrorCode,
                        failure.ErrorMessage));
                }
            }
        }

        private static string JoinFieldPath(string prefix, string field) =>
            string.IsNullOrEmpty(field) ? prefix : $"{prefix}.{field}";
    }
}
