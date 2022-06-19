namespace Ptiw.HostApp.Tasks.CheckNpcpnSchedule.Responses
{
    internal class AppointableSeance
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        public DateTimeOffset Time { get; set; }
    }
}