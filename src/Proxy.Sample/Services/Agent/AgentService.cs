using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microblink.Platform.Proxy.Sample;

public class AgentService : IAgentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private ApiClientCredentials _apiClientCredentials;

    public AgentService(IHttpClientFactory httpClientFactory, IOptions<ApiClientCredentials> options)
    {
        _httpClientFactory = httpClientFactory;
        _apiClientCredentials = options.Value;
    }

    public async Task<HttpResponseMessage> ProcessRequest(string url, HttpRequest request, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = _apiClientCredentials.Address;

        // Build the outgoing request
        using var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method),
            _apiClientCredentials.Address.AbsoluteUri + url);

        // Set our authorization
        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_apiClientCredentials.ClientId}:{_apiClientCredentials.ClientSecret}")));

        // Add body content for methods that support it
        if (IsMutation(request))
        {
            requestMessage.Content = new StreamContent(request.Body);
            ForwardMutationHeaders(request, requestMessage);
        }

        var response = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, ct);

        return response;
    }

    private static bool IsMutation(HttpRequest request)
        => HttpMethods.IsPost(request.Method) ||
           HttpMethods.IsPut(request.Method) ||
           HttpMethods.IsPatch(request.Method);


    private static void ForwardMutationHeaders(HttpRequest request, HttpRequestMessage requestMessage)
    {
        if (requestMessage.Content == null)
            return;

        // Forward content-specific headers
        if (request.ContentLength.HasValue)
            requestMessage.Content.Headers.ContentLength = request.ContentLength.Value;

        if (!string.IsNullOrEmpty(request.ContentType))
            requestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", request.ContentType);

        // Forward other content headers
        foreach (var header in request.Headers)
        {
            if (header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase) &&
                !header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) &&
                !header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }
    }
}