using Catalog.Domain.Airlines;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public sealed class AirlineConfiguration : IEntityTypeConfiguration<Airline>
{
    public void Configure(EntityTypeBuilder<Airline> builder)
    {
        builder.ToTable("airlines");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("code")
            .HasConversion(code => code.Value, value => new AirlineCode(value))
            .HasMaxLength(3);

        builder.Property(a => a.Name).HasColumnName("name").IsRequired();
    }
}
