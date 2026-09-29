using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public sealed class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.ToTable("flights");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new FlightId(value));

        builder.OwnsOne(f => f.Number, number =>
        {
            number.Property(n => n.Number).HasColumnName("flight_number");
            number.Property(n => n.Airline)
                .HasColumnName("airline_code")
                .HasConversion(code => code.Value, value => new AirlineCode(value))
                .HasMaxLength(3);
        });

        // Volvió a HasConversion escalar (ver FlightSearchReadModel para el motivo):
        // ni así ni como owned type EF Core logra traducir una comparación de
        // igualdad sobre AirportCode a SQL, así que la owned-type no compraba nada
        // extra y sí agregaba complejidad de mapeo.
        builder.Property(f => f.Origin)
            .HasColumnName("origin_code")
            .HasConversion(code => code.Value, value => new AirportCode(value))
            .HasMaxLength(3);

        builder.Property(f => f.Destination)
            .HasColumnName("destination_code")
            .HasConversion(code => code.Value, value => new AirportCode(value))
            .HasMaxLength(3);

        builder.Property(f => f.AircraftType)
            .HasColumnName("aircraft_type_id")
            .HasConversion(id => id.Value, value => new AircraftTypeId(value));

        // Normaliza a UTC al escribir: Npgsql rechaza cualquier DateTimeOffset con
        // offset distinto de cero contra "timestamp with time zone" (probado en vivo
        // contra Postgres real -- tira ArgumentException, no es una suposición).
        // Esto en realidad confirma que guardar hora local con su offset fijo en el
        // vuelo hubiera sido redundante: la hora local de verdad se reconstruye
        // combinando este instante UTC con el TimeZone del Airport de origen, no
        // guardando un offset que además podría desactualizarse con cambios de DST.
        builder.Property(f => f.DepartureAt)
            .HasColumnName("departure_at")
            .HasConversion(dt => dt.ToUniversalTime(), dt => dt);

        builder.Property(f => f.ArrivalAt)
            .HasColumnName("arrival_at")
            .HasConversion(dt => dt.ToUniversalTime(), dt => dt);

        // FareClasses vive detrás de una lista privada (_fareClasses): Flight es el
        // agregado raíz y nadie debería poder reemplazar la colección desde afuera,
        // ni siquiera EF. UsePropertyAccessMode(Field) le dice a EF que lea/escriba
        // el campo directamente en vez de buscar un setter público que no existe.
        builder.Navigation(f => f.FareClasses).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(f => f.FareClasses, fareClasses =>
        {
            fareClasses.ToTable("fare_classes");
            fareClasses.WithOwner().HasForeignKey("FlightId");
            fareClasses.HasKey(fc => fc.Id);

            fareClasses.Property(fc => fc.Id)
                .HasColumnName("id")
                .HasConversion(id => id.Value, value => new FareClassId(value));

            fareClasses.Property(fc => fc.Cabin)
                .HasColumnName("cabin")
                .HasConversion<string>()
                .HasMaxLength(20);

            fareClasses.OwnsOne(fc => fc.BasePrice, price =>
            {
                price.Property(p => p.Amount).HasColumnName("base_price").HasColumnType("numeric(10,2)");
                price.Property(p => p.Currency).HasColumnName("currency").HasMaxLength(3);
            });

            fareClasses.Property(fc => fc.AvailableSeats).HasColumnName("available_seats");
        });
    }
}
