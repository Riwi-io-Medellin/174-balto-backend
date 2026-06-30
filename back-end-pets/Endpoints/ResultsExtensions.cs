using BackEndPets.Application.DTOs.Common;

namespace BackEndPets.API.Endpoints;

/// <summary>
/// Helper para el caso "catch-all" de los switch de errorCode en los endpoints.
/// Sustituye los `_ => Results.StatusCode(StatusCodes.Status500InternalServerError)`
/// (sin body) por una respuesta consistente con ApiErrorResponse.
///
/// Uso: cambiar
///     _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
/// por
///     _ => Results.UnhandledError()
/// </summary>
public static class ResultsExtensions
{
    public static IResult UnhandledError() =>
        Results.Json(
            new ApiErrorResponse("An unexpected error occurred. Please try again later.", "UNHANDLED_ERROR"),
            statusCode: StatusCodes.Status500InternalServerError);
}
