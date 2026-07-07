using System.Net;
using BackEndPets.Application.DTOs.Common;
using BackEndPets.Application.DTOs.Pets;
using BackEndPets.Application.Interfaces;

namespace BackEndPets.API.Endpoints;

/// <summary>
/// Unauthenticated endpoints for NFC pet tags: a finder taps the tag and lands here
/// with no Balto account required, either as JSON (in-app) or a plain HTML page (browser).
/// </summary>
public static class PetTagEndpoints
{
    public static IEndpointRouteBuilder MapPetTagEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/pet-tag/{id:guid}", async (Guid id, IPetService service) =>
        {
            var info = await service.GetPublicTagInfoAsync(id);
            return info is null
                ? Results.NotFound(new ApiErrorResponse("Pet not found.", "PET_NOT_FOUND"))
                : Results.Ok(info);
        })
        .WithTags("PetTag")
        .WithName("GetPublicPetTagInfo")
        .WithSummary("Get a pet's public NFC-tag info (no auth required)")
        .Produces<PublicPetTagResponse>(StatusCodes.Status200OK)
        .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound);

        app.MapGet("/pet-tag/{id:guid}", async (Guid id, IPetService service) =>
        {
            var info = await service.GetPublicTagInfoAsync(id);
            return Results.Content(RenderHtml(info), "text/html");
        })
        .WithTags("PetTag")
        .WithName("GetPublicPetTagPage")
        .WithSummary("Public HTML page shown when an NFC tag is tapped in a browser");

        return app;
    }

    private static string RenderHtml(PublicPetTagResponse? p)
    {
        if (p is null)
        {
            return """
                <!doctype html><html><head><meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>Pet not found · Balto</title></head>
                <body style="font-family:sans-serif;text-align:center;padding:40px;color:#1A1A2E;">
                <h2>This tag isn't linked to a pet yet.</h2></body></html>
                """;
        }

        var name = WebUtility.HtmlEncode(p.Name);
        var species = WebUtility.HtmlEncode(p.Species ?? "Unknown species");
        var breed = p.Breed is null ? "" : WebUtility.HtmlEncode(p.Breed);
        var color = p.Color is null ? "" : WebUtility.HtmlEncode(p.Color);
        var ownerName = WebUtility.HtmlEncode(p.OwnerName);
        var ownerPhone = WebUtility.HtmlEncode(p.OwnerPhone);
        var photo = string.IsNullOrWhiteSpace(p.PhotoUrl) ? "" : WebUtility.HtmlEncode(p.PhotoUrl);
        var age = p.BirthDate is null ? null : (int?)((DateTime.UtcNow - p.BirthDate.Value).TotalDays / 365);

        var lostBanner = p.IsLost
            ? """<div style="background:#FDECEA;color:#D32F2F;font-weight:700;padding:12px;border-radius:12px;margin-bottom:16px;">⚠️ This pet has been reported lost. Please contact the owner.</div>"""
            : "";

        var photoHtml = photo == ""
            ? $"""<div style="width:120px;height:120px;border-radius:50%;background:#EEF3F3;display:flex;align-items:center;justify-content:center;font-size:48px;color:#3A80C2;margin:0 auto 16px;">{name[..1].ToUpperInvariant()}</div>"""
            : $"""<img src="{photo}" style="width:120px;height:120px;border-radius:50%;object-fit:cover;margin-bottom:16px;">""";

        var callButton = string.IsNullOrWhiteSpace(p.OwnerPhone)
            ? ""
            : $"""<a href="tel:{ownerPhone}" style="display:block;background:#1BAA71;color:white;text-decoration:none;font-weight:700;padding:14px;border-radius:12px;margin-top:16px;">Call {ownerName}</a>""";

        var details = string.Join("", new[]
        {
            breed == "" ? null : $"<div><b>Breed:</b> {breed}</div>",
            color == "" ? null : $"<div><b>Color:</b> {color}</div>",
            age is null ? null : $"<div><b>Age:</b> {age} yr</div>",
        }.Where(s => s is not null));

        return $"""
            <!doctype html><html><head><meta charset="utf-8">
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{name} · Balto Pet Tag</title></head>
            <body style="font-family:sans-serif;max-width:420px;margin:0 auto;padding:32px 20px;color:#1A1A2E;text-align:center;">
            {lostBanner}
            {photoHtml}
            <h1 style="margin:0 0 4px;">{name}</h1>
            <div style="color:#6B7280;margin-bottom:16px;">{species}</div>
            <div style="text-align:left;background:#F0F4F4;border-radius:14px;padding:16px;line-height:1.8;">
              {details}
              <div><b>Owner:</b> {ownerName}</div>
            </div>
            {callButton}
            <p style="color:#9CA3AF;font-size:12px;margin-top:24px;">Scanned via a Balto NFC pet tag.</p>
            </body></html>
            """;
    }
}
