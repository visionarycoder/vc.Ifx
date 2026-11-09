using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Ifx.Component;

public class ComponentRequest<T> : ComponentRequest
{
    public ComponentRequest(ILogger<ComponentRequest> logger) : base(logger)
    {
    }
}
