using Microsoft.Extensions.Logging;

namespace Ptiw.Libs.Telegram.Logging;
public record TelegramLoggerOptions
{
    public TelegramLoggerOptions()
        : this(Microsoft.Extensions.Logging.LogLevel.Information)
    {
    }

    public TelegramLoggerOptions(LogLevel logLevel)
    {
        LogLevel = new() { { "Default", logLevel } };
    }

    public Dictionary<string, LogLevel> LogLevel { get; set; }
    public string AccessToken { get; set; }
    public string ChatId { get; set; }
    public bool UseEmoji { get; set; } = true;
    public string Source { get; set; }
}