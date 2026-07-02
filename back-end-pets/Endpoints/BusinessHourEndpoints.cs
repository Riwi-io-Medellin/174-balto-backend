using System.Security.Claims;
using BackEndPets.Application.DTOs.Businesses;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackEndPets.API.Endpoints;

public static class BusinessHourEndpoints
{
    public static IEndpointRouteBuilder MapBusinessHourEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/businesses/me/hours")
            .WithTags("Businesses")
            .RequireAuthorization();

        group.MapGet("/", async (HttpContext ctx, IBusinessHourService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var (result, errorCode) = await service.GetMyHoursAsync(userId);
            return errorCode switch
            {
                "BUSINESS_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Business profile not found.", "BUSINESS_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetMyBusinessHours")
        .Produces<IReadOnlyCollection<BusinessHourResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/", async (
            HttpContext ctx, [FromBody] IEnumerable<BusinessHourRequest> hours, IBusinessHourService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var (result, errorCode) = await service.ReplaceMyHoursAsync(userId, hours);
            return errorCode switch
            {
                "BUSINESS_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Business profile not found.", "BUSINESS_NOT_FOUND")),
                "BUSINESS_NOT_APPROVED" => Results.Conflict(
                    new ApiErrorResponse("Only approved businesses can manage hours.", "BUSINESS_NOT_APPROVED")),
                "INVALID_DAY_OF_WEEK" => Results.BadRequest(
                    new ApiErrorResponse("dayOfWeek must be between 0 and 6.", "INVALID_DAY_OF_WEEK")),
                "INVALID_TIME_RANGE" => Results.BadRequest(
                    new ApiErrorResponse("startTime must be earlier than endTime.", "INVALID_TIME_RANGE")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("ReplaceMyBusinessHours")
        .Produces<IReadOnlyCollection<BusinessHourResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/exceptions", async (HttpContext ctx, IBusinessHourService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var (result, errorCode) = await service.GetMyExceptionsAsync(userId);
            return errorCode switch
            {
                "BUSINESS_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Business profile not found.", "BUSINESS_NOT_FOUND")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("GetMyBusinessHourExceptions")
        .Produces<IReadOnlyCollection<BusinessHourExceptionResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        group.MapPut("/exceptions", async (
            HttpContext ctx, [FromBody] IEnumerable<BusinessHourExceptionRequest> exceptions, IBusinessHourService service) =>
        {
            var userIdStr = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Results.Unauthorized();

            var (result, errorCode) = await service.ReplaceMyExceptionsAsync(userId, exceptions);
            return errorCode switch
            {
                "BUSINESS_NOT_FOUND" => Results.NotFound(
                    new ApiErrorResponse("Business profile not found.", "BUSINESS_NOT_FOUND")),
                "BUSINESS_NOT_APPROVED" => Results.Conflict(
                    new ApiErrorResponse("Only approved businesses can manage hours.", "BUSINESS_NOT_APPROVED")),
                "DUPLICATE_DATE" => Results.BadRequest(
                    new ApiErrorResponse("Each date must appear only once.", "DUPLICATE_DATE")),
                "TIMES_REQUIRED_WHEN_AVAILABLE" => Results.BadRequest(
                    new ApiErrorResponse("startTime/endTime required when isUnavailable is false.", "TIMES_REQUIRED_WHEN_AVAILABLE")),
                "INVALID_TIME_RANGE" => Results.BadRequest(
                    new ApiErrorResponse("startTime must be earlier than endTime.", "INVALID_TIME_RANGE")),
                _ when result is not null => Results.Ok(result),
                _ => ResultsExtensions.UnhandledError()
            };
        })
        .WithName("ReplaceMyBusinessHourExceptions")
        .Produces<IReadOnlyCollection<BusinessHourExceptionResponse>>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
        .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict);

        return app;
    }
}