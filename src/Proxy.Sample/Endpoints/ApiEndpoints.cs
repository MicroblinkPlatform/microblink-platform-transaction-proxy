using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Microblink.Platform.Proxy.Sample;

public static class ApiEndpoints
{
    public static void ConfigureRoutes(this WebApplication app)
    {
        app.MapTransaction();
    }

    private static void MapTransaction(this WebApplication app)
    {
        app.MapPost("/transaction", async (IAgentService proxy, [FromBody] CreateTransactionRequest request, CancellationToken ct) =>
        {
                try
                {
                    CreateTransactionResponse response = await proxy.StartTransaction(Enrich(request), ct);
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Failed to create transaction",
                        detail: ex.Message,
                        statusCode: StatusCodes.Status400BadRequest);
                }

        }).WithName("StartTransaction");
    }

    private static CreateTransactionRequest Enrich(CreateTransactionRequest request)
    {
        // here you can modify incoming request before passing it through
        return request;
    }
}
