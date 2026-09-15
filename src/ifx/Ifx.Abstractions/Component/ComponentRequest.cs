using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Ifx.Component;


/// <summary>Compatible inheritable marker for component requests; carries no implicit payload.</summary>
public class ComponentRequest(ILogger<ComponentRequest> logger) : ComponentBase<ComponentRequest>(logger)
{
}
