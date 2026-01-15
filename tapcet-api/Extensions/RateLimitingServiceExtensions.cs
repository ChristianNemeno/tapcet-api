using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace tapcet_api.Extensions;

public static class RateLimitingServiceExtensions
{
    public static IServiceCollection AddRateLimitingServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddRateLimiter(options =>
        {
            // Named policies (usable via [EnableRateLimiting("...")])
            options.AddFixedWindowLimiter("public", limiterOptions =>
            {
                limiterOptions.PermitLimit = config.GetValue<int>("RateLimiting:PublicEndpoints:PermitLimit");
                limiterOptions.Window = TimeSpan.FromSeconds(config.GetValue<int>("RateLimiting:PublicEndpoints:Window"));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = config.GetValue<int>("RateLimiting:PublicEndpoints:QueueLimit");
            });

            options.AddFixedWindowLimiter("authenticated", limiterOptions =>
            {
                limiterOptions.PermitLimit = config.GetValue<int>("RateLimiting:AuthenticatedEndpoints:PermitLimit");
                limiterOptions.Window = TimeSpan.FromSeconds(config.GetValue<int>("RateLimiting:AuthenticatedEndpoints:Window"));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = config.GetValue<int>("RateLimiting:AuthenticatedEndpoints:QueueLimit");
            });

            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = config.GetValue<int>("RateLimiting:AuthEndpoints:PermitLimit");
                limiterOptions.Window = TimeSpan.FromSeconds(config.GetValue<int>("RateLimiting:AuthEndpoints:Window"));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = config.GetValue<int>("RateLimiting:AuthEndpoints:QueueLimit");
            });

            // Global limiter: authenticated => per user, otherwise per IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: $"user:{userId}",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = config.GetValue<int>("RateLimiting:AuthenticatedEndpoints:PermitLimit"),
                            Window = TimeSpan.FromSeconds(config.GetValue<int>("RateLimiting:AuthenticatedEndpoints:Window")),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = config.GetValue<int>("RateLimiting:AuthenticatedEndpoints:QueueLimit")
                        });
                }

                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"ip:{ipAddress}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = config.GetValue<int>("RateLimiting:PublicEndpoints:PermitLimit"),
                        Window = TimeSpan.FromSeconds(config.GetValue<int>("RateLimiting:PublicEndpoints:Window")),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = config.GetValue<int>("RateLimiting:PublicEndpoints:QueueLimit")
                    });
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var response = new
                {
                    message = "Too many requests. Please try again later.",
                    retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? retryAfter.TotalSeconds
                        : 60
                };

                await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            };
        });

        return services;
    }
}
