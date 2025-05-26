using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microblink.Platform.Proxy.Sample;

public class Program
{

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder
            .UseCommon()
            .UseMvc();

        ConfigureHost(builder);

        builder.Services.AddOptions<ApiClientCredentials>()
            .Bind(builder.Configuration.GetSection("ApiClientCredentials"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddSingleton<IValidateOptions<ApiClientCredentials>, ApiClientCredentials>();

        builder.Services.AddTransient<IAgentService,AgentService>();
        builder.Services.AddTransient<IAuthService, AuthService>();

        var app = builder.Build();

        app.UsePathBase(builder.Configuration.GetValue<string>("ASPNETCORE_BASEPATH"));

        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.ConfigureRoutes();

        app.Run();
    }

    private static void ConfigureHost(WebApplicationBuilder builder)
    {
        // Note: Reduce request limits if not submitting files during transaction start phase.
        // If submitting files, increase limits to allow larger files.

        builder.WebHost.ConfigureKestrel((context, serverOptions) =>
        {
            serverOptions.AddServerHeader = false;
            serverOptions.Limits.MaxRequestBodySize = 20 * 1024 * 1024; // 20 MB
        });

        builder.Services.Configure<FormOptions>(options =>
        {
            options.ValueCountLimit = 1024;
            options.ValueLengthLimit = 20 * 1024 * 1024;
            options.MultipartBodyLengthLimit = 20 * 1024 * 1024;
            options.MemoryBufferThreshold = 20 * 1024 * 1024; // If disk is readonly, need to keep it fully in memory, or mount external disk
        });
    }
}
