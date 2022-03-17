using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace ManagedCode.Repository.Core.Common;

public class TimeMetrics : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _method;
    private readonly Stopwatch _sw;

    private TimeMetrics(ILogger logger, string method)
    {
        _logger = logger;
        _method = method;
        _sw = new Stopwatch();
        _sw.Start();
    }

    public void Dispose()
    {
        _sw.Stop();
        _logger.LogInformation($"{_method} was performed in {_sw.Elapsed}");
    }

    public static IDisposable GetTimer(ILogger logger, string className, [CallerMemberName] string method = null)
    {
        return new TimeMetrics(logger, string.Join('.', className, method));
    }
}