using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Ptiw.Settings
{
    public class AppSettings
    {
        public string TelegramToken { get; set; }
        public long Wife_ID { get; set; }
        public long Husband_ID { get; set; }
        public string ServiceContext { get; set; }
        public Dictionary<string, JObject> Tasks { get; set; }
    }
}