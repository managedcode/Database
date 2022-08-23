using System;
using Microsoft.Extensions.Logging;
using LogLevel = Tenray.ZoneTree.Core.LogLevel;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class WrapperLogger : Tenray.ZoneTree.ILogger
{
    private readonly ILogger _logger;

    public WrapperLogger(ILogger logger)
    {
        _logger = logger;
    }
    public void LogError(Exception log)
    {
        _logger.LogError(log, log.Message);
    }

    public void LogWarning(object log)
    {
        if (log is Exception ex)
        {
            _logger.LogWarning(ex, ex.Message);
        }
        else
        {
            _logger.LogWarning(log.ToString());
        }
        
    }

    public void LogInfo(object log)
    {
        _logger.LogInformation(log.ToString());
    }

    public void LogTrace(object log)
    {
        _logger.LogTrace(log.ToString());
    }

    public LogLevel LogLevel { get; set; }
}