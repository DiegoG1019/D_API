using System.Text.Json;
using System.Text.Json.Serialization;
using static DiegoG.Utilities.IO.Serialization;

namespace D_API.Types.Utilities;

[CustomConverter]
public class GuidJsonConverter : JsonConverter<Guid>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEquivalentTo(typeof(Guid));

    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? str;
        return (str = reader.GetString()) is not null ? Guid.Parse(str) : Guid.Empty;
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options) => writer.WriteStringValue(value);
}
