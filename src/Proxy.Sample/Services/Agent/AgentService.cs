using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Microblink.Platform.Proxy.Sample;

public class AgentService : IAgentService
{
    private readonly ILogger _log;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthService _authProxy;


    public AgentService(
              ILogger<AgentService> log,
              IHttpClientFactory httpClientFactory,
              IAuthService authProxy)
    {
        _httpClientFactory = httpClientFactory;
        _log = log;
        _authProxy = authProxy;
    }

    public async Task<CreateTransactionResponse> StartTransaction(CreateTransactionRequest request, CancellationToken ct)
    {
        _log.LogInformation("Starting transaction for {Request}.", request);


        var token = await _authProxy.GetAccessToken(ct);
        
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        client.BaseAddress = _authProxy.Address;


        var response = await client.PostAsJsonAsync("api/v1/transaction", request, ct);

        if (response.IsSuccessStatusCode)
        {
            var transaction = await response.Content.ReadFromJsonAsync<CreateTransactionResponse>(ct);
            if (transaction != null)
            {
                _log.LogInformation("Transaction {TransactionId} created.", transaction.TransactionId);
                return transaction;
            }
            else
            {
                _log.LogError("Transaction create succeeded, but could not read transaction object.");
                throw new Exception("Failed to deserialize transaction object.");
            }
        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            if (!string.IsNullOrEmpty(errorBody))
            {
                try
                {
                    var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(errorBody);
                    if (problemDetails != null)
                    {
                        _log.LogError("Transaction invocation failed: {StatusCode}. {Problem}.", response.StatusCode, problemDetails);
                        throw new Exception(problemDetails.Detail ?? problemDetails.Title ?? "Transaction invocation failed.");
                    }
                    else
                    {
                        _log.LogError("Transaction invocation failed: {StatusCode}. Did not receive problem details.", response.StatusCode);
                        throw new Exception("Transaction invocation failed. Did not receive problem details.");
                    }
                }
                catch
                {
                }
            }
            _log.LogError("Transaction invocation failed: {StatusCode}. Did not receive problem details. Raw response: {ErrorBody}", response.StatusCode, errorBody);
            throw new Exception("Transaction invocation failed. Did not receive problem details. Raw response logged.");
        }
    }
}
