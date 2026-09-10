---
title: Azure AD Authentication Flow Patterns
description: Compact Microsoft Entra ID setup snippets for ASP.NET Core web apps, APIs, daemon apps, roles, and OBO flows.
doc_type: reference
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: medium
estimated_tokens: 1090
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - azure-ad-authentication-dotnet
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - azure
  - entra
  - authentication
  - reference
---
# Azure AD Authentication Flow Patterns

## Web App Pattern

```csharp
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
```

## Web API Pattern

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddDownstreamWebApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
    .AddInMemoryTokenCaches();
```

## Daemon Pattern

```csharp
var app = ConfidentialClientApplicationBuilder
    .Create(configuration["AzureAd:ClientId"])
    .WithClientSecret(configuration["AzureAd:ClientSecret"])
    .WithAuthority(new Uri($"https://login.microsoftonline.com/{configuration["AzureAd:TenantId"]}"))
    .Build();

var result = await app.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();
```

## Authorization Matrix

| Need | Pattern |
|---|---|
| Role gate | `[Authorize(Roles = "Admin")]` |
| Policy gate | `options.AddPolicy("RequireAdminRole", p => p.RequireRole("Admin"));` |
| Scope gate | Validate accepted scopes in API auth configuration |

## Tenant Validation Pattern

```csharp
options.Events = new OpenIdConnectEvents
{
    OnTokenValidated = context =>
    {
        var tenantId = context.SecurityToken.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;
        if (!AllowedTenants.Contains(tenantId, StringComparer.Ordinal))
        {
            context.Fail("Tenant not authorized");
        }

        return Task.CompletedTask;
    }
};
```

## OBO Pattern

| Need | Pattern |
|---|---|
| Downstream user token | `ITokenAcquisition.GetAccessTokenForUserAsync(scopes)` |
| API wrapper | `IDownstreamWebApi.CallWebApiForUserAsync(...)` |
| Safer config | Keep downstream scopes in config |
