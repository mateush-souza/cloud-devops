using challenge_moto_connect.Domain.Entity;
using challenge_moto_connect.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace challenge_moto_connect.Infrastructure.Persistence.Context
{
    public class ChallengeMotoConnectContext(DbContextOptions<ChallengeMotoConnectContext> options) : DbContext(options)
    {
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<TelemetryData> TelemetryData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new VehicleMapping());
            modelBuilder.ApplyConfiguration(new UserMapping());
            modelBuilder.ApplyConfiguration(new HistoryMapping());
            modelBuilder.Entity<TelemetryData>().ToTable("TelemetryData");
        }
    }
}


