namespace Ptiw.HostApp.Tasks.CheckNpcpnSchedule.Responses
{
    internal class AppointableDoctor
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("patronymic")]
        public string Patronymic { get; set; }

        [JsonProperty("info")]
        public string Info { get; set; }

        internal string GetFullName()
        {
            return $"{LastName} {FirstName} {Patronymic ?? string.Empty}";
        }
    }
}