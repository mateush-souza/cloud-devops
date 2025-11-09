using System.Reflection;
using challenge_moto_connect.Application.Services;
using challenge_moto_connect.Domain.Interfaces;
using challenge_moto_connect.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using challenge_moto_connect.Domain.Entity;
using challenge_moto_connect.Infrastructure.Persistence.Context;
using challenge_moto_connect.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using challenge_moto_connect.Api.HealthChecks;
using System.Linq;

namespace challenge_moto_connect
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddNewtonsoftJson();

            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            var allowedOriginsRaw = builder.Configuration["Cors:AllowedOrigins"];

            if (!string.IsNullOrWhiteSpace(allowedOriginsRaw))
            {
                var runtimeOrigins = allowedOriginsRaw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                allowedOrigins = allowedOrigins.Concat(runtimeOrigins).ToArray();
            }

            allowedOrigins = allowedOrigins
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DefaultCorsPolicy", policy =>
                {
                    if (allowedOrigins.Length == 0 || allowedOrigins.Contains("*", StringComparer.OrdinalIgnoreCase))
                    {
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        policy.WithOrigins(allowedOrigins);
                    }

                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.WithExposedHeaders("X-Pagination");
                });
            });

            builder.Services.AddApiVersioning(config =>
            {
                config.DefaultApiVersion = new ApiVersion(1, 0);
                config.AssumeDefaultVersionWhenUnspecified = true;
                config.ReportApiVersions = true;
                config.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

            builder.Services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddHealthChecks()
                .AddDbContextCheck<ChallengeMotoConnectContext>(name: "database", tags: new[] { "ready" })
                .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "ready" })
                .AddCheck<CpuHealthCheck>("cpu", tags: new[] { "ready" })
                .AddDiskStorageHealthCheck(options => options.AddDrive("/", 1024), name: "disk", tags: new[] { "ready" })
                .AddProcessAllocatedMemoryHealthCheck(512, name: "process_memory", tags: new[] { "live" });

            builder.Services.AddEndpointsApiExplorer();

            var jwtKey = builder.Configuration["Jwt:Key"] ?? "EstaEChaveSecretaParaJWTChallengeMotoConnectComMinimoTrintaEDoisCaracteres2024";
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MotoConnectIssuer";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MotoConnectAudience";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });
            builder.Services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new OpenApiInfo
                    {
                    Title = builder.Configuration["Swagger:Title"] ?? "Moto Connect - Challenge",
                        Version = "v1",
                    Description = builder.Configuration["Swagger:Description"] ?? "API para gerenciamento de motocicletas",
                    Contact = new OpenApiContact()
                    {
                        Name = "Moto Connect",
                        Email = builder.Configuration["Swagger:Email"] ?? "rm558424@fiap.com.br",
                    },
                });

                x.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = builder.Configuration["Swagger:Title"] ?? "Moto Connect - Challenge",
                    Version = "v2",
                    Description = "API v2 com melhorias e novos recursos",
                        Contact = new OpenApiContact()
                        {
                            Name = "Moto Connect",
                        Email = builder.Configuration["Swagger:Email"] ?? "rm558424@fiap.com.br",
                        },
                });

                x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Insira o token JWT no formato: Bearer {seu token}"
                });

                x.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    x.IncludeXmlComments(xmlPath);
                }
            });

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IHistoryService, HistoryService>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();
            builder.Services.AddSingleton<IMLService, MLService>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ChallengeMotoConnectContext>();
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    
                    logger.LogInformation("Verificando conexão com banco de dados...");
                    
                    var maxRetries = 5;
                    var retryCount = 0;
                    var connected = false;
                    
                    while (retryCount < maxRetries && !connected)
                    {
                        try
                        {
                            connected = context.Database.CanConnect();
                            if (connected)
                            {
                                logger.LogInformation("Banco de dados conectado com sucesso.");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            logger.LogWarning("Tentativa {RetryCount}/{MaxRetries} de conexão falhou: {Message}", retryCount, maxRetries, ex.Message);
                            if (retryCount < maxRetries)
                            {
                                Thread.Sleep(TimeSpan.FromSeconds(5));
                            }
                        }
                    }
                    
                    if (connected)
                    {
                        logger.LogInformation("Aplicando migrations...");
                    context.Database.Migrate();
                        logger.LogInformation("Migrations aplicadas com sucesso.");
                    }
                    else
                    {
                        logger.LogError("Não foi possível conectar ao banco de dados após {MaxRetries} tentativas. A aplicação continuará, mas migrations não foram aplicadas.", maxRetries);
                    }
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Erro ao aplicar migrations durante startup: {Message}. StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                }
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Moto Connect API v1");
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "Moto Connect API v2");
                c.RoutePrefix = "swagger";
                c.DisplayRequestDuration();
            });

            app.UseCors("DefaultCorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapControllers();

            app.MapPost("/api/telemetry", async (TelemetryData data, ChallengeMotoConnectContext context) =>
            {
                context.TelemetryData.Add(data);
                await context.SaveChangesAsync();
                return Results.Created($"/api/telemetry/{data.Id}", data);
            });

            app.MapGet(
                "/",
                context =>
                {
                    context.Response.Redirect("/swagger", permanent: false);
                    return Task.CompletedTask;
                }
            );

            app.MapGet("/error", () => Results.Problem("Ocorreu um erro interno no servidor."));

            app.Run();
        }
    }
}
