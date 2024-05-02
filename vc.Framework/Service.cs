using Microsoft.Extensions.Logging;

namespace vc.Framework;

/// <summary>
/// Base class for service components.
/// Leverage Microsoft.Extensions.Logging.ILogger
/// </summary>
/// <typeparam name="T"></typeparam>
public class Service<T>(ILogger logger)
	where T : IService, new()
{

}