using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microblink.Platform.Proxy.Sample;

public static class ApiEndpoints
{
    public static void ConfigureRoutes(this WebApplication app)
    {
        // Changed logic to restream the response from the agent API instead of deserializing
        // and reserializing the content. This allows for better performance and support for streaming responses.

        // The endpoints are defined to match the agent API routes, and the proxy service is used to forward the requests and responses.

        // With this change just by adding new routes we aim to achive better extensibility & maintainability of the proxy,
        // as we won't need to change the core logic of the proxy to support new endpoints, but just add new route mappings here.

        // Detailed API deatils and routs can be seen on the agent API documentation: https://platform.docs.microblink.com/api 
        // or https://api.us-east.platform.microblink.com/agent/api/v1/schema.json where <us-east> should be replaced with requested region
        app.MapTransaction();
        app.MapInitializeInfo();
        app.MapInitializeCancel();
    }

    private static void MapTransaction(this WebApplication app)
    {
        app.MapPost("/transaction", async (IAgentService proxy, HttpRequest request, CancellationToken ct) =>
        {
            try
            {
                var response = await proxy.ProcessRequest("api/v1/transaction", request, ct);
                return await ForwardResponse(response, ct);
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

    private static void MapInitializeInfo(this WebApplication app)
    {
        app.MapGet("/initialize/{workflowId}/info", async (string workflowId, IAgentService proxy, HttpRequest request, CancellationToken ct) =>
        {
            try
            {
                var response = await proxy.ProcessRequest($"api/v1/initialize/{workflowId}/info", request, ct);
                return await ForwardResponse(response, ct);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Failed to initialize transaction",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }).WithName("GetInitializationInfo");
    }

    private static void MapInitializeCancel(this WebApplication app)
    {
        app.MapPost("/initialize/{workflowId}/cancel", async (string workflowId, IAgentService proxy, HttpRequest request, CancellationToken ct) =>
        {
            try
            {
                var response = await proxy.ProcessRequest($"api/v1/initialize/{workflowId}/cancel", request, ct);
                return await ForwardResponse(response, ct);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Failed to cancel workflow",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }).WithName("CancelWorkflow");
    }

    /// <summary>
    /// Forwards the HttpResponseMessage content to the client.
    /// </summary>      
    private static async Task<IResult> ForwardResponse(HttpResponseMessage response, CancellationToken ct)
    {
        var content = await response.Content.ReadAsStreamAsync(ct);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";

        return Results.Stream(content, contentType);
    }
}
