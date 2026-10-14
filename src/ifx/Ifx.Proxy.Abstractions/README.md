# Ifx.Proxy.Abstractions

## Mission

Define the proxy invocation contracts without the default pipeline, DI registration or
interceptor dependencies. Transport and interceptor authors reference this package alone.

## Contents

`IProxyPipeline`, `IProxyTransport`, `IProxyInterceptor`, `IOrderedProxyInterceptor`,
`IProxyErrorClassifier`, `ProxyDelegate<T>`, `ProxyContext`, `ProxyResponse<T>`,
`ProxyHeaders`, `ProxyOptions`, `ProxyInterceptorOrderAttribute` and the proxy exception types.
Types keep the `Ifx.Proxy` namespaces, so consumers of `Ifx.Proxy` need no `using` changes.
`Ifx.Proxy` supplies `DefaultProxyPipeline` and service registration.