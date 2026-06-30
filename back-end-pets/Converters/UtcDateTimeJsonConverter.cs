using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackEndPets.API.Converters;

public class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dt = reader.GetDateTime();
        // Preserve Unspecified kind (Colombia local time); only enforce UTC when Z/offset is explicit.
        return dt.Kind == DateTimeKind.Unspecified ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value); // Utc → "…Z", Unspecified → no suffix, Local → offset
}
