#pragma warning disable CS9113 // Parameter is unread.
using Microsoft.Extensions.Logging;

namespace vc.Ifx.Svc;

/// <summary>
/// Base class for service components.
/// Leverage Microsoft.Extensions.Logging.ILogger
/// </summary>
/// <typeparam name="T"></typeparam>
public class Service<T>(ILogger<T> logger)
    where T : IService, new()
{

}