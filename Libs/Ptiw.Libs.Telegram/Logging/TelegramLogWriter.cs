using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Ptiw.Libs.Telegram.Logging;

public interface ILogWriter
{
    Task Write(string message);
}

public class TelegramLogWriter : ILogWriter
{
    private readonly string _chatId;
    private readonly ITelegramBotClient _client;

    public TelegramLogWriter(string accessToken, string chatId)
        : this(new TelegramBotClient(accessToken), chatId)
    {
    }

    public TelegramLogWriter(ITelegramBotClient client, string chatId)
    {
        _chatId = chatId;
        _client = client;
    }

    public async Task Write(string message) =>
        await _client.SendTextMessageAsync(_chatId, message, ParseMode.Html);
}