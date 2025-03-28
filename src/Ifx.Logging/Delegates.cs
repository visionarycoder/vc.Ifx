using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifx.Logging;

public delegate void LogCritical(string message, params object[] args);
public delegate void LogDebug(string message, params object[] args);
public delegate void LogError(string message, params object[] args);
public delegate void LogInformation(string message, params object[] args);
public delegate void LogTrace(string message, params object[] args);
