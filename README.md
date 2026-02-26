<p align="center" >
  <img src="https://raw.githubusercontent.com/wiki/blinkid/blinkid-android/images/logo-microblink.png" alt="Microblink" title="Microblink">
</p>

# Microblink Platform transaction proxy

This repository contains a lightweight .NET 9 proxy service packaged as an OCI-compliant container. 

This README covers local testing and development, and doesn't include deployment to external hosts.

**Note**: This proxy service is for demonstration purposes only and should not be used in production environments.

## Prerequisites

To successfully run the proxy, you need:

1. A [Microblink Platform account and API credentials](https://platform.docs.microblink.com/account-setup/). After this step, you should know your region, as well as your client ID and client secret.
2. A [workflow](https://platform.docs.microblink.com/build-workflow/).
3. An [SDK](https://platform.docs.microblink.com/sdk-integration/) with which you can test the workflow. Alternatively, to just check if the proxy works, you can skip this step and test it manually with `curl` or a similar tool.

You'll also need a tool to create OCI images and containers, such as Docker or Podman. We'll use the `podman` CLI tool in these instructions, but the commands would also work with `docker` CLI.

Follow the links above to set everything up first, then clone this repository and configure settings.

## Configuration

Configure the proxy in `src/Proxy.Sample/appsettings.json`, under `ApiClientCredentials`:

```json
{
    "ApiClientCredentials": {
        "Address": "https://api.us-east.platform.microblink.com/agent/",
        "ClientId": "example-client-id",
        "ClientSecret": "example-client-secret"
```

- The `Address` field should match the region you picked when creating your organization. Find the root address for your region [here](https://platform.docs.microblink.com/api/#regions). Don't forget to append `/agent`! For example:
  - `https://api.us-east.platform.microblink.com/agent/`
  - `https://api.eu.platform.microblink.com/agent/`
- The `ClientId` and `ClientSecret` values should match the values you got when you created API credentials.

### CORS configuration

If you're testing this with a browser SDK, you'll also need to configure CORS headers, otherwise you won't be able to initiate a transaction. 

In `src/Proxy.Sample/Program.cs`, add the following two lines:

```cs
builder.Services.AddCors();
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()); 
```

So that the final portion of Program.cs looks like this:

```cs
        builder.Services.AddTransient<IAgentService,AgentService>();
        builder.Services.AddTransient<IAuthService, AuthService>();
        builder.Services.AddCors();
        var app = builder.Build();

        app.UsePathBase(builder.Configuration.GetValue<string>("ASPNETCORE_BASEPATH"));

        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()); 
        app.ConfigureRoutes();

        app.Run();
    }
```

This is not necessary if you're not using the browser SDK to test.

## Image and container

In the root of the repository, create an image from the Dockerfile: 

```bash
podman build --tag transaction-proxy src
```

Then, run the container. Map your machine's port 8081 to the container's port 8080:

```bash
podman run --publish 8081:8080 transaction-proxy
```

This makes the container available on localhost:8081 and ready to start proxying SDK requests.

## Endpoints

The proxy exposes the following endpoints, each forwarding to the corresponding Agent API route:

| Method | Proxy route | Agent API route |
|--------|-------------|-----------------|
| `POST` | `/transaction` | `api/v1/transaction` |
| `GET`  | `/initialize/{workflowId}/info` | `api/v1/initialize/{workflowId}/info` |
| `POST` | `/initialize/{workflowId}/cancel` | `api/v1/initialize/{workflowId}/cancel` |

For full details on request/response shapes, see the [Agent API documentation](https://platform.docs.microblink.com/api) or the OpenAPI schema at `https://api.us-east.platform.microblink.com/agent/api/v1/schema.json` (replace `us-east` with your region).

## Testing the proxy

### Using the web SDK

To fully (locally) test the functionality of the proxy, the best way is to run one of our [example web apps](https://github.com/MicroblinkPlatform/microblink-platform-browser-sdk/tree/main/example-react). This allows you to test different capabilities.

Follow the instructions in the README for that repository, then pass the workflow ID and the proxy URL (`http://localhost:8081/transaction`).

In your browser's developer tools, under the network tab, you should see the response from the proxy containing the full Agent API response.

### Using `curl`

If you just want to verify that the proxy is operational using `curl`, follow [this tutorial](https://platform.docs.microblink.com/api/transaction-api), but instead of contacting the Agent API directly, contact your proxy at `http://localhost:8081/transaction`.

Create a `request_body.json` file. Here's an example one:

```json
{
  "workflowId": "6870ca44335606082bb4bf90",
  "platform": "browser",
  "sdkVersion": "1.4.0",
  "consent": {
    "userId": "unique-user-id",
    "givenOn": "2025-08-29T09:00:00",
    "isProcessingStoringAllowed": true,
    "isTrainingAllowed": true
  }
}
```

Change the contents to match your workflow. If unsure, see the tutorial linked above.

Then, run the `curl` command:

```bash
curl --url http://localhost:8081/transaction --json @request_body.json
```

The proxy service will send your request to the Agent API and return data back:

```json
{
  "transactionId": "0168b84a94eb57fb65879eb3f5",
  "workflowId": "6870ca44335606082bb4bf90",
  "workflowVersionId": "68b58647b84a9097911b3f30",
  "organizationId": "66d99fa9edc165df54072f8c",
  "ephemeralKey": "r3RvKMXepniMAX2HV8MRLZH3M8gm6sD..."
  "authorization": "MDE2OGI4NGE5NGViNTdmYjY1ODc5Zs..."
  "workflowInfo": {
    "stepCount": 2,
    "interactiveStepCount": 0,
    "hasConditionalInteractiveStep": false,
    "interactiveSteps": [],
    "completedInteractiveSteps": [],
    "currentStep": "End",
    "currentStepRetryCount": 0,
    "currentStepExecutionIndex": 1,
    "steps": [],
    "currentStepId": 2,
    "pendingStepIds": [],
    "completedStepIds": []
  },
  "createdOn": "2025-09-03T14:03:00.7029384Z",
  "modifiedOn": "2025-09-03T14:03:00.707429Z",
  "processingStatus": "InProgress",
  "warnings": [],
  "edgeApiUrl": "https://api.us-east.platform.microblink.com/edge"
}
```

This confirms that the proxy is operational, that it has successfully contacted the Agent API, and has returned information on where to continue with the transaction. (All of this happens behind the scenes when using an SDK.)

