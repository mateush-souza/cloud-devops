using System;
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
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' não foi encontrada. Verifique as configurações.");
            }

            services.AddDbContext<ChallengeMotoConnectContext>(options =>
            {
                options.UseSqlServer(
                    connectionString,
                    x => x.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            return services;
        }
    }
}


