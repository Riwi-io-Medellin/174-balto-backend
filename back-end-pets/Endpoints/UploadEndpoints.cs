using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BackEndPets.API.Endpoints;

public static class UploadEndpoints
{
    public static IEndpointRouteBuilder MapUploadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/upload")
            .WithTags("Upload")
            .RequireAuthorization();

        group.MapPost("/", async (HttpRequest request, Cloudinary cloudinary) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest(new { error = "Expected multipart form.", code = "INVALID_CONTENT_TYPE" });

            var form = await request.ReadFormAsync();
            var file = form.Files.GetFile("file");
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "No file provided.", code = "FILE_REQUIRED" });

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "pet_photos",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await cloudinary.UploadAsync(uploadParams);

            if (result.Error is not null)
                return Results.Json(
                    new { error = result.Error.Message, code = "CLOUDINARY_ERROR" },
                    statusCode: 500);

            return Results.Ok(new { url = result.SecureUrl.ToString() });
        })
        .WithName("UploadImage")
        .WithSummary("Upload an image to Cloudinary")
        .Produces<UploadResponse>(StatusCodes.Status200OK)
        .Produces<UploadError>(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();

        return app;
    }
}

public sealed record UploadResponse(string Url);

public sealed record UploadError(string Error, string Code);
