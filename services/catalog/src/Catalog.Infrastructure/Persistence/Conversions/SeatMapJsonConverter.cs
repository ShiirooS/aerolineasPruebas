using System.Text.Json;
using System.Text.Json.Serialization;
using Catalog.Domain.Aircraft;
using Catalog.Domain.Flights;

namespace Catalog.Infrastructure.Persistence.Conversions;

internal static class SeatMapJsonConverter
{
    // JsonStringEnumConverter para que la columna jsonb quede legible
    // ({"Economy":150}) en vez de claves numéricas ({"0":150}).
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Serialize(SeatMap seatMap) =>
        JsonSerializer.Serialize(seatMap.ToDictionary(entry => entry.Key, entry => entry.Value), Options);

    public static SeatMap Deserialize(string json) =>
        new(JsonSerializer.Deserialize<Dictionary<CabinClass, int>>(json, Options)!);
}
