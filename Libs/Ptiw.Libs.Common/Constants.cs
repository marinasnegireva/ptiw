using System.Globalization;

namespace Ptiw.Libs.Common
{
    public static class Constants
    {
        public static CultureInfo RuCulture = new("ru-RU");

        public static class SettingNames
        {
            public const string TelegramToken = "TelegramToken";
            public const string AdminTelegramToken = "AdminTelegramToken";
            public const string AdminUserId = "AdminUserId";
            public const string ServiceContext = "ServiceContext";
            public const string Jobs = "Jobs";
            public const string Enabled = "Enabled";
            public const string URL = "URL";
        }
    }
}