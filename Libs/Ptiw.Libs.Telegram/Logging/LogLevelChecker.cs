using Microsoft.Extensions.Logging;

namespace Ptiw.Libs.Telegram.Logging;

public interface ILogLevelChecker
{
    bool IsEnabled(LogLevel logLevel);
}

public class DefaultLogLevelChecker : ILogLevelChecker
{
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
}