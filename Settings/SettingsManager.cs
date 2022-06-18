using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Ptiw.Settings
{
    public static class SettingsManager
    {
        //  private static AppSettings appSettings;

        public static AppSettings AppSettings
        {
            get => JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(@"Settings.json"));
        }

        public static List<long> AllowedUserIds
        {
            get => new List<long> { AppSettings.Husband_ID, AppSettings.Wife_ID };
        }
    }
}