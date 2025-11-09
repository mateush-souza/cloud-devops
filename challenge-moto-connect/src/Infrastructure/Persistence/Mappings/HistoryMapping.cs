using challenge_moto_connect.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace challenge_moto_connect.Infrastructure.Persistence.Mappings
{
    /// <summary>
    /// Mapeamento da entidade MaintenanceHistory para o banco de dados.
    /// </summary>
    public class HistoryMapping : IEntityTypeConfiguration<History>
    {
        public void Configure(EntityTypeBuilder<History> builder)
        {
            builder.ToTable("MaintenanceHistories");


            builder.HasKey(m => m.MaintenanceHistoryID);

            builder.Property(m => m.MaintenanceHistoryID)
                .IsRequired();

            builder.Property(m => m.VehicleID)
                .IsRequired();

            builder.Property(m => m.UserID)
                .IsRequired();

            builder.Property(m => m.MaintenanceDate)
                .IsRequired();

            builder.Property(m => m.StatusModel)
                .IsRequired();

            builder.Property(m => m.Description)
                .IsRequired()
                .HasMaxLength(500);

        }
    }
}
