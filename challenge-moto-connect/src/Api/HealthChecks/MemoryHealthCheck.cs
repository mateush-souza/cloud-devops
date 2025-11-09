using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace challenge_moto_connect.Api.HealthChecks
{
    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly long _threshold;

        public MemoryHealthCheck(long threshold = 1024 * 1024 * 1024)
        {
            _threshold = threshold;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var allocated = GC.GetTotalMemory(false);
            var data = new Dictionary<string, object>
            {
                { "AllocatedBytes", allocated },
                { "Gen0Collections", GC.CollectionCount(0) },
                { "Gen1Collections", GC.CollectionCount(1) },
                { "Gen2Collections", GC.CollectionCount(2) }
            };

            var status = allocated < _threshold ? HealthStatus.Healthy : HealthStatus.Degraded;

            return Task.FromResult(new HealthCheckResult(
                status,
                description: $"Memória alocada: {allocated / 1024 / 1024} MB",
                data: data
            ));
        }
    }
}

