using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ptiw.Libs.EF;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Ptiw.Libs.Common.Constants;

namespace Ptiw.Libs.Telegram
{
    public class BotWorker : BackgroundService
    {
        private readonly ILogger<BotWorker> _logger;
        private readonly TelegramBotClient _telegramBotClient;
        private readonly IConfiguration _configuration;
        private readonly ServiceContext _serviceContext;

        public BotWorker(ILogger<BotWorker> logger, IConfiguration configuration, ServiceContext serviceContext)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceContext = serviceContext;
            _telegramBotClient = new TelegramBotClient(_configuration[SettingNames.TelegramToken]);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
            if (!IsUserAllowed(update.Message.From.Id))
            {
                await botClient.SendTextMessageAsync(update?.Message?.Chat, "You are not in the user list!");
                return;
            }
            if (update.Message is Message message)
            {
                //process it
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

        private bool IsUserAllowed(long? id)
        {
            if (id == null) return false;
            if (_configuration[SettingNames.AdminUserId] == id.ToString())
                return true;
            return _serviceContext.Users.Any(u => u.TelegramId == id);
        }
    }
}