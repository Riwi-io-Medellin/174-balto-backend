using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Entities;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BackEndPets.Infrastructure.Services;

public sealed class QuestPdfClinicalDocumentGenerationService(Cloudinary? cloudinary)
    : IPetClinicalDocumentGenerationService
{
    public async Task<string> GenerateAsync(
        Pet pet,
        PetClinicalRecord record,
        IReadOnlyCollection<PetClinicalEvent> events,
        string ownerFullName,
        string? ownerPhone,
        string? ownerAddress,
        string? ownerEmail)
    {
        if (cloudinary is null)
            throw new InvalidOperationException("Cloudinary is not configured; cannot store the generated PDF.");

        var orderedEvents = events.OrderBy(e => e.EventDate).ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("Balto — Historia Clínica Oficial").FontSize(18).Bold();
                    col.Item().Text($"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).FontColor(Colors.Grey.Medium);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Element(c => SectionTitle(c, "Datos de la mascota"));
                    col.Item().Text($"Nombre: {pet.Name}    Especie: {pet.Species ?? "N/A"}    Raza: {pet.Breed ?? "N/A"}");
                    col.Item().Text($"Sexo: {pet.Sex ?? "N/A"}    Color: {pet.Color ?? "N/A"}    Peso: {pet.Weight?.ToString("0.##") ?? "N/A"} kg");
                    col.Item().Text($"Nacimiento: {pet.BirthDate?.ToString("yyyy-MM-dd") ?? "N/A"}    Identificación: {pet.IdentificationNumber ?? "N/A"}    Microchip: {pet.MicrochipNumber ?? "N/A"}");

                    col.Item().Element(c => SectionTitle(c, "Datos del propietario"));
                    col.Item().Text($"Nombre: {ownerFullName}    Teléfono: {ownerPhone ?? "N/A"}");
                    col.Item().Text($"Dirección: {ownerAddress ?? "N/A"}    Correo: {ownerEmail ?? "N/A"}");

                    col.Item().Element(c => SectionTitle(c, "Resumen médico"));
                    col.Item().Text($"Alergias: {record.Allergies ?? "Ninguna registrada"}");
                    col.Item().Text($"Condiciones crónicas: {record.ChronicConditions ?? "Ninguna registrada"}");
                    col.Item().Text($"Restricciones alimenticias: {record.DietaryRestrictions ?? "Ninguna registrada"}");

                    col.Item().Element(c => SectionTitle(c, "Vacunas"));
                    AppendEventList(col, orderedEvents.Where(e => e.EventType == PetClinicalEventType.Vaccine));

                    col.Item().Element(c => SectionTitle(c, "Cirugías"));
                    AppendEventList(col, orderedEvents.Where(e => e.EventType == PetClinicalEventType.Surgery));

                    col.Item().Element(c => SectionTitle(c, "Medicamentos"));
                    foreach (var med in orderedEvents.SelectMany(e => e.Medications))
                        col.Item().Text($"• {med.Name} — {med.Dose ?? "N/A"}, {med.Frequency ?? "N/A"}, {med.Duration ?? "N/A"}");

                    col.Item().Element(c => SectionTitle(c, "Cronología de eventos clínicos"));
                    foreach (var e in orderedEvents)
                    {
                        col.Item().Text($"{e.EventDate:yyyy-MM-dd} — {e.EventType.ToUpperInvariant()} — {e.Reason ?? e.Diagnosis ?? "Sin motivo registrado"}").Bold();
                        if (!string.IsNullOrWhiteSpace(e.Diagnosis)) col.Item().Text($"   Diagnóstico: {e.Diagnosis}");
                        if (!string.IsNullOrWhiteSpace(e.ExamResults)) col.Item().Text($"   Resultados: {e.ExamResults}");
                        if (!string.IsNullOrWhiteSpace(e.Recommendations)) col.Item().Text($"   Recomendaciones: {e.Recommendations}");
                        if (!string.IsNullOrWhiteSpace(e.Observations)) col.Item().Text($"   Observaciones: {e.Observations}");
                        if (e.NextControlDate is not null) col.Item().Text($"   Próximo control: {e.NextControlDate:yyyy-MM-dd}");
                    }

                    col.Item().Element(c => SectionTitle(c, "Última actualización"));
                    col.Item().Text($"{record.UpdatedAt:yyyy-MM-dd HH:mm} UTC");
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Documento generado automáticamente por Balto a partir de información estructurada. ");
                    t.Span("No reemplaza el criterio de un médico veterinario.").FontColor(Colors.Grey.Medium);
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        stream.Position = 0;

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription($"clinical-history-{pet.Id}.pdf", stream),
            Folder = "balto/clinical-history",
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await cloudinary.UploadAsync(uploadParams);
        return result.SecureUrl.ToString();
    }

    private static void SectionTitle(QuestPDF.Infrastructure.IContainer container, string title) =>
        container.Text(title).FontSize(13).Bold().FontColor(Colors.Blue.Darken2);

    private static void AppendEventList(QuestPDF.Fluent.ColumnDescriptor col, IEnumerable<PetClinicalEvent> events)
    {
        var list = events.ToList();
        if (list.Count == 0)
        {
            col.Item().Text("Ninguna registrada.").FontColor(Colors.Grey.Medium);
            return;
        }
        foreach (var e in list)
            col.Item().Text($"• {e.EventDate:yyyy-MM-dd} — {e.Reason ?? e.Diagnosis ?? "Sin detalle"}");
    }
}
