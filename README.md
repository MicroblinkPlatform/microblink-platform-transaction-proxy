<p align="center" >
  <img src="https://raw.githubusercontent.com/wiki/blinkid/blinkid-android/images/logo-microblink.png" alt="Microblink" title="Microblink">
</p>

# Microblink Platform Transaction Proxy

Microblink Platform Transaction Proxy is lightweight .NET 9 service for secure and seamless authentication with Microblink Platform. 

## Table Of Contents
- [Installation](#installation)
- [Configuration](#configuration)
- [Provisioning](#provisioning)

## Installation
1. Pull the code from repository

2. Provide [Configuration](#configuration) values from Microblink Platform Dasbhboard - create API keys with **transaction.execute** permission.

3. Proxy API test - start the service and call it from postman

URL 
```http
POST http://localhost:2105/transaction 
# Change url to match one which you have set up in launchsettings.json
Content-Type: application/json
```
Body
```json
{
    "workflowId": "67a651aa5782356731276b99d", // Use your workflowId from Microblink Platform Dashboard
    "platform": "browser", 
    "sdkVersion": "0.1.0", 
    "formFields": null
}
```

4. Response will return populated data
 ```json
{
    "transactionId": "",
    "workflowId": "67a651aa5782356731276b99d",
    "workflowVersionId": "",
    "organizationId": "",
    "ephemeralKey": "",
    "authorization": "", 
    "workflowInfo": {
        "stepCount": 5,
        "interactiveStepCount": 2,
        "hasConditionalInteractiveStep": false,
        "interactiveSteps": [ // Depends on workflow setup
            "DocVer",
            "FaceMatch"
        ],
        "completedInteractiveSteps": [],
        "currentStep": "Start",
        "currentStepRetryCount": 0,
        "currentStepExecutionIndex": 1
    },
    "createdOn": "",
    "modifiedOn": "",
    "processingStatus": "Pending",
    "warnings": [],
    "edgeApiUrl": "https://api.*.microblink.com/edge"
}
```

5. At this point Proxy API works. Expose the port to ensure your service is available outside of the server and provide SDKs with your new URL.
    

## Configuration
Microblink Platform Transaction Proxy configuration is done by adding configuration values in appsettings.json

|Property         |    Type    | Required | Example |       Description                    |
| ----------------|------------|----------| ---------------------------------------------------- | ------------- |
|`Address`        | `string`   |   Yes    | `https://api.*.microblink.com/agent/` | Region specific URL |
| `ClientId` | `string` | Yes | `QRl1pF4o1GQUPhfNaLjptL2PV7FCk2` | Generate on Dashboard |
| `ClientSecret` |`string` | Yes | `5eVolxANEx83732B5WFJgepzC2ovkh` |Generate on Dashboard


# Provisioning
**Address** - depends on the region where Organization is hosted. 
* US-East - https://api.us-east.platform.microblink.com/agent/api/
* Brazil  - https://api.br.platform.microblink.com/agent/api/

**ClientId** - provided by Microblink Platform Dasbhboard

**ClientSecret** - provided by Microblink Platform Dashboard