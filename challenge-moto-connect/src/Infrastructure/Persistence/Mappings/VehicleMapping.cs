using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using challenge_moto_connect.Domain.Entity;

namespace challenge_moto_connect.Infrastructure.Persistence.Mappings
{
    public class VehicleMapping : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicles");
            builder.HasKey(v => v.VehicleId);

            builder.Property(v => v.VehicleId)
                   .IsRequired();

            builder.Property(v => v.LicensePlate)
                   .IsRequired()
                   .HasMaxLength(8);

            builder.Property(v => v.VehicleModel)
                   .IsRequired();
        }
    }
}