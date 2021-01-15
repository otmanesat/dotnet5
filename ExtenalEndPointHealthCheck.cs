using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HelloDotNet5
{
    public class ExtenalEndPointHealthCheck : IHealthCheck
    {
        private readonly ServiceSettings options;
        public ExtenalEndPointHealthCheck(IOptions<ServiceSettings> options)
        {
            this.options = options.Value;

        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            Ping ping = new();
            var reply = await ping.SendPingAsync(options.ApiUrl);
            if (reply.Status != IPStatus.Success){
                return HealthCheckResult.Unhealthy();
            }
            return HealthCheckResult.Healthy();
            
        }
    }
}