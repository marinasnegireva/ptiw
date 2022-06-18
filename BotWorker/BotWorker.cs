using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ptiw.Settings;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Ptiw.HostApp
{
    public class BotWorker : BackgroundService
    {
        private readonly ILogger<BotWorker> _logger;
        private readonly TelegramBotClient _telegramBotClient;

        public BotWorker(ILogger<BotWorker> logger, TelegramBotClient telegramBotClient)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            //quartz.SchedulerName = "esf";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var settings = SettingsManager.AppSettings;
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[] { UpdateType.Message } // receive all update types
            };
            _telegramBotClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            //_telegramBotClient.

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogTrace("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(10000, stoppingToken);
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (!SettingsManager.AllowedUserIds.Contains(update.Message.From.Id))
            {
                return;
            }
            if (update.Message is Message message)
            {
                await botClient.SendTextMessageAsync(message.Chat, "Hello");
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception.ToString());
            /* if (exception is ApiRequestException apiRequestException)
             {
                 await botClient.SendTextMessageAsync(123, apiRequestException.ToString());
             }*/
        }
    }
}