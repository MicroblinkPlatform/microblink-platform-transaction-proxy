using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microblink.Platform.Proxy.Sample;

internal static class ServiceConfiguration
{
    /// <summary>
    /// Configures common services.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder UseCommon(this IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.AddHttpLogging(logging =>
        {
            logging.RequestBodyLogLimit = 4096;
            logging.ResponseBodyLogLimit = 4096;
            logging.CombineLogs = true;
        });


        builder.Services.AddHttpClient();
        builder.Services.AddMemoryCache();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddHealthChecks();

        return builder;
    }

    /// <summary>
    /// Configures MVC services.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder UseMvc(this IHostApplicationBuilder builder)
    {
        // Minimal API Requires this
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = (ctx) =>
            {
                ctx.ProblemDetails.Type = "https://httpstatuses.com/" + ctx.HttpContext.Response.StatusCode;
                ctx.ProblemDetails.Extensions.Remove("traceId");
                ctx.ProblemDetails.Extensions.Add("traceId", Activity.Current != null ? Activity.Current.TraceId.ToString() : ctx.HttpContext.TraceIdentifier);
            };
        });

        builder.Services.AddHttpContextAccessor();


        return builder;
    }
}
