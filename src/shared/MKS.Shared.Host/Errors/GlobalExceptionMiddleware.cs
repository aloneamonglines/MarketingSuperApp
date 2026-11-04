using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Domain.Shared.Constants;

namespace MKS.Shared.Host.Errors
{
    /// <summary>Converts application failures into the common API error envelope.</summary>
    public sealed class GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context, IApiErrorMessageLocalizer localizer)
        {
            try
            {
                await next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (!context.Response.HasStarted)
            {
                var (statusCode, error) = CreateError(exception, context, localizer);
                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(
                    ApiResponse.Failure(error, context.TraceIdentifier),
                    context.RequestAborted);
            }
        }

        private (int StatusCode, ApiError Error) CreateError(
            Exception exception,
            HttpContext context,
            IApiErrorMessageLocalizer localizer)
        {
            switch (exception)
            {
                case DomainException domainException:
                    logger.LogInformation(
                        "Request rejected by domain rule {ErrorCode}. TraceId: {TraceId}",
                        domainException.LocalizationKey,
                        context.TraceIdentifier);
                    return (
                        StatusCodes.Status400BadRequest,
                        new ApiError(
                            domainException.LocalizationKey,
                            localizer.Localize(
                                domainException.LocalizationKey,
                                domainException.LocalizationArguments),
                            []));

                case RequestValidationException validationException:
                    logger.LogInformation(
                        "Request validation failed with {FailureCount} errors. TraceId: {TraceId}",
                        validationException.Failures.Count,
                        context.TraceIdentifier);
                    return (
                        StatusCodes.Status400BadRequest,
                        new ApiError(
                            "Shared.Errors.ValidationFailed",
                            localizer.Localize("Shared.Errors.ValidationFailed"),
                            validationException.Failures
                                .Select(failure => new ApiErrorDetail(
                                    failure.Field,
                                    failure.Code,
                                    localizer.Localize(failure.Code)))
                                .ToArray()));

                case ValidationException fluentValidationException:
                    return CreateError(
                        new RequestValidationException(fluentValidationException.Errors
                            .Select(failure => new ApiValidationFailure(
                                failure.PropertyName,
                                failure.ErrorCode,
                                failure.ErrorMessage))
                            .ToArray()),
                        context,
                        localizer);

                case KeyNotFoundException:
                    logger.LogInformation(
                        "Requested resource was not found. TraceId: {TraceId}",
                        context.TraceIdentifier);
                    return (
                        StatusCodes.Status404NotFound,
                        new ApiError(
                            "Shared.Errors.NotFound",
                            localizer.Localize("Shared.Errors.NotFound"),
                            []));

                case UnauthorizedAccessException:
                    logger.LogInformation(
                        "Request was forbidden. TraceId: {TraceId}",
                        context.TraceIdentifier);
                    return (
                        StatusCodes.Status403Forbidden,
                        new ApiError(
                            "Shared.Errors.Forbidden",
                            localizer.Localize("Shared.Errors.Forbidden"),
                            []));

                default:
                    logger.LogError(
                        exception,
                        "Unhandled request exception. TraceId: {TraceId}",
                        context.TraceIdentifier);
                    return (
                        StatusCodes.Status500InternalServerError,
                        new ApiError(
                            "Shared.Errors.Internal",
                            localizer.Localize("Shared.Errors.Internal"),
                            []));
            }
        }
    }
}
