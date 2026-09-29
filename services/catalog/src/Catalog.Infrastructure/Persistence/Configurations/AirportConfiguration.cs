using Catalog.Domain.Airports;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public sealed class AirportConfiguration : IEntityTypeConfiguration<Airport>
{
    public void Configure(EntityTypeBuilder<Airport> builder)
    {
        builder.ToTable("airports");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("code")
            .HasConversion(code => code.Value, value => new AirportCode(value))
            .HasMaxLength(3);

        builder.Property(a => a.Name).HasColumnName("name").IsRequired();
        builder.Property(a => a.City).HasColumnName("city").IsRequired();
        builder.Property(a => a.Country).HasColumnName("country").IsRequired();

        builder.Property(a => a.TimeZone)
            .HasColumnName("timezone")
            .HasConversion(timeZone => timeZone.Id, id => TimeZoneInfo.FindSystemTimeZoneById(id));
    }
}
