using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace challenge_moto_connect.Api.HealthChecks
{
    public class CpuHealthCheck : IHealthCheck
    {
        private readonly int _threshold;

        public CpuHealthCheck(int threshold = 80)
        {
            _threshold = threshold;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var process = Process.GetCurrentProcess();
            var cpuUsage = process.TotalProcessorTime.TotalMilliseconds / (Environment.ProcessorCount * process.PrivilegedProcessorTime.TotalMilliseconds) * 100;

            var data = new Dictionary<string, object>
            {
                { "ProcessorCount", Environment.ProcessorCount },
                { "TotalProcessorTime", process.TotalProcessorTime.TotalMilliseconds }
            };

            var status = cpuUsage < _threshold ? HealthStatus.Healthy : HealthStatus.Degraded;

            return Task.FromResult(new HealthCheckResult(
                status,
                description: $"CPU: {Environment.ProcessorCount} cores",
                data: data
            ));
        }
    }
}

