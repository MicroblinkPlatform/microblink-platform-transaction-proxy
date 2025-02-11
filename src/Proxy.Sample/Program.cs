using Microsoft.AspNetCore.Builder;
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


        builder.Services.AddOptions<ApiClientCredentials>()
            .Bind(builder.Configuration.GetSection("ApiClientCredentials"))
            .ValidateDataAnnotations();

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
}
