using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApiPrototype.HealthChecks
{
    public class MyHealthCheck : IHealthCheck
    {
        /// <summary>
        /// Custom heath check showing two different ways to report back a status.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var isHealthy = false;

            // perform some processing to check the health of something

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("A healthy result."));
            }

            var data = new Dictionary<string, object>()
            {
                { "SomeKey", 17 },
                { "AnotherKey", "a string value" },
            };

            return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, "An unhealthy result.", null, data));

            // if this method throws an exception, a FailureStatus is returned (unhealthy), with the inner exception in the Description

        }
    }
}
