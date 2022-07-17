using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Ptiw.Libs.Telegram.Logging;

internal class TelegramLoggerProvider : ILoggerProvider
{
    private readonly ILogQueueProcessor _logQueueProcessor;
    private readonly ILogLevelChecker _logLevelChecker;
    private readonly TelegramLoggerOptions _options;
    private readonly ConcurrentDictionary<string, TelegramLogger> _loggers = new();
    private readonly Func<string, ITelegramMessageFormatter> _createFormatter;

    public TelegramLoggerProvider(
        TelegramLoggerOptions options,
        ILogQueueProcessor logQueueProcessor,
        ILogLevelChecker logLevelChecker,
        Func<string, ITelegramMessageFormatter> createFormatter)
    {
        _options = options;
        _logQueueProcessor = logQueueProcessor;
        _logLevelChecker = logLevelChecker;
        _createFormatter = createFormatter;
    }

    public ILogger CreateLogger(string name) => _loggers.GetOrAdd(name, CreateTelegramLogger);

    private TelegramLogger CreateTelegramLogger(string name) =>
        new TelegramLogger(_options, _logLevelChecker, _logQueueProcessor, _createFormatter(name));

    public void Dispose() => _loggers.Clear();
}