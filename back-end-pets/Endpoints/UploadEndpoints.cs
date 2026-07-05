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

        group.MapPost("/", async (HttpRequest request, Cloudinary? cloudinary) =>
        {
            if (cloudinary is null)
                return Results.Json(
                    new { error = "Upload service not configured.", code = "UPLOAD_NOT_CONFIGURED" },
                    statusCode: 500);
        
            if (!request.HasFormContentType)
                return Results.BadRequest(new { error = "Expected multipart form.", code = "INVALID_CONTENT_TYPE" });
        
            var form = await request.ReadFormAsync();
            var file = form.Files.GetFile("file");
        
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "No file provided.", code = "FILE_REQUIRED" });
        
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var videoExtensions = new[] { ".mp4", ".mov", ".webm", ".m4v", ".3gp" };
            var allowedExtensions = imageExtensions.Concat(videoExtensions).Append(".pdf");
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return Results.BadRequest(new UploadError(
                    "Invalid file type. Allowed: jpg, jpeg, png, webp, gif, mp4, mov, webm, m4v, 3gp, pdf.",
                    "INVALID_FILE_TYPE"));

            var isVideo = videoExtensions.Contains(extension);
            var maxBytes = isVideo ? 50 * 1024 * 1024 : 10 * 1024 * 1024;
            if (file.Length > maxBytes)
                return Results.BadRequest(new UploadError(
                    isVideo ? "File exceeds 50MB limit." : "File exceeds 10MB limit.",
                    "FILE_TOO_LARGE"));

            await using var stream = file.OpenReadStream();
            var isImage = imageExtensions.Contains(extension);

            UploadResult result;

            if (isImage)
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "balto",
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                result = await cloudinary.UploadAsync(uploadParams);
            }
            else if (isVideo)
            {
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "balto/videos",
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                result = await cloudinary.UploadAsync(uploadParams);
            }
            else
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "balto/documents",
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };
                result = await cloudinary.UploadAsync(uploadParams);
            }
        
            if (result.Error is not null)
                return Results.Json(
                    new UploadError(result.Error.Message, "CLOUDINARY_ERROR"),
                    statusCode: 500);
        
            return Results.Ok(new UploadResponse(result.SecureUrl.ToString()));
        })
        .WithName("UploadFile")
        .WithSummary("Upload an image or document to Cloudinary")
        .Produces<UploadResponse>(StatusCodes.Status200OK)
        .Produces<UploadError>(StatusCodes.Status400BadRequest)
        .DisableAntiforgery();

        return app;
    }
}

public sealed record UploadResponse(string Url);

public sealed record UploadError(string Error, string Code);
