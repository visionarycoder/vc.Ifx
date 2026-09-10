# vc.Ifx.WebApi

`vc.Ifx.WebApi` provides ASP.NET Core Web API infrastructure artifacts for framework-level applications.

The package focuses on cross-cutting HTTP concerns that should be consistent across services: global exception handling, Problem Details responses, and reusable resilience pipelines for outbound or volatile operations.

## Usage

```csharp
builder.Services.AddIfxWebApi(options =>
{
    options.IncludeExceptionDetails = builder.Environment.IsDevelopment();
});

var app = builder.Build();

app.UseIfxWebApiExceptionHandling();
```

Use `IWebApiResiliencePipelineFactory` to create Polly resilience pipelines for operations that need retry and timeout behavior.
