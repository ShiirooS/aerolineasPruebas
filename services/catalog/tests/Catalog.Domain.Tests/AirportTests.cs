using Catalog.Domain.Airports;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Tests;

public class AirportTests
{
    private static Airport CreateAirport(string code, string timeZoneId) =>
        new(new AirportCode(code), "Aeropuerto", "Ciudad", "País", TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));

    [Fact]
    public void LocalDayBoundsUtc_uses_the_airport_local_day_not_the_utc_day()
    {
        var lima = CreateAirport("LIM", "America/Lima");

        var (start, end) = lima.LocalDayBoundsUtc(new DateOnly(2026, 10, 10));

        Assert.Equal(new DateTimeOffset(2026, 10, 10, 5, 0, 0, TimeSpan.Zero), start);
        Assert.Equal(new DateTimeOffset(2026, 10, 11, 5, 0, 0, TimeSpan.Zero), end);
    }

    [Fact]
    public void LocalDayBoundsUtc_on_a_day_whose_midnight_is_skipped_by_dst_starts_at_the_jump()
    {
        // Chile entra en horario de verano el 6/9/2026: a las 24:00 del sábado el
        // reloj salta a las 01:00, así que la medianoche del domingo no existe.
        var santiago = CreateAirport("SCL", "America/Santiago");

        var (start, end) = santiago.LocalDayBoundsUtc(new DateOnly(2026, 9, 6));

        Assert.Equal(new DateTimeOffset(2026, 9, 6, 4, 0, 0, TimeSpan.Zero), start);
        Assert.Equal(new DateTimeOffset(2026, 9, 7, 3, 0, 0, TimeSpan.Zero), end);
        Assert.Equal(TimeSpan.FromHours(23), end - start);
    }

    [Fact]
    public void LocalDayBoundsUtc_on_a_day_that_gains_an_hour_lasts_25_hours()
    {
        // Chile sale del horario de verano el 5/4/2026: a las 24:00 del sábado 4 el
        // reloj vuelve a las 23:00, así que ese sábado dura 25 horas.
        var santiago = CreateAirport("SCL", "America/Santiago");

        var (start, end) = santiago.LocalDayBoundsUtc(new DateOnly(2026, 4, 4));

        Assert.Equal(new DateTimeOffset(2026, 4, 4, 3, 0, 0, TimeSpan.Zero), start);
        Assert.Equal(TimeSpan.FromHours(25), end - start);
    }

    [Fact]
    public void LocalDayBoundsUtc_on_a_day_whose_midnight_repeats_starts_at_the_first_occurrence()
    {
        // Cuba sale del horario de verano el 1/11/2026: a la 01:00 el reloj vuelve a
        // las 00:00, así que la medianoche ocurre dos veces (primero a -4, luego a -5).
        var havana = CreateAirport("HAV", "America/Havana");

        var (start, end) = havana.LocalDayBoundsUtc(new DateOnly(2026, 11, 1));

        Assert.Equal(new DateTimeOffset(2026, 11, 1, 4, 0, 0, TimeSpan.Zero), start);
        Assert.Equal(new DateTimeOffset(2026, 11, 2, 5, 0, 0, TimeSpan.Zero), end);
    }

    [Fact]
    public void ToLocalTime_applies_the_airport_offset()
    {
        var lima = CreateAirport("LIM", "America/Lima");

        var local = lima.ToLocalTime(new DateTimeOffset(2026, 10, 11, 3, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateTimeOffset(2026, 10, 10, 22, 30, 0, TimeSpan.FromHours(-5)), local);
        Assert.Equal(TimeSpan.FromHours(-5), local.Offset);
    }
}
