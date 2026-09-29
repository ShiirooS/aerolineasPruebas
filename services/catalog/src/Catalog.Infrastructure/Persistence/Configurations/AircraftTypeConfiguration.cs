using Catalog.Domain.Aircraft;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence.Conversions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public sealed class AircraftTypeConfiguration : IEntityTypeConfiguration<AircraftType>
{
    public void Configure(EntityTypeBuilder<AircraftType> builder)
    {
        builder.ToTable("aircraft_types");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => new AircraftTypeId(value));

        builder.Property(a => a.Model).HasColumnName("model").IsRequired();

        builder.Property(a => a.SeatMap)
            .HasColumnName("seat_map_json")
            .HasColumnType("jsonb")
            .HasConversion(
                seatMap => SeatMapJsonConverter.Serialize(seatMap),
                json => SeatMapJsonConverter.Deserialize(json),
                new ValueComparer<SeatMap>(
                    (a, b) => a!.Equals(b),
                    a => a.GetHashCode()));
    }
}
