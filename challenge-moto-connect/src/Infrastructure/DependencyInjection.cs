using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using challenge_moto_connect.Infrastructure.Persistence.Context;
using challenge_moto_connect.Domain.Interfaces;
using challenge_moto_connect.Infrastructure.Persistence.Repositories;

namespace challenge_moto_connect.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ChallengeMotoConnectContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}


