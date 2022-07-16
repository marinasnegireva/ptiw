namespace Ptiw.HostApp.Tasks.CheckNpcpnSchedule.ResponseModels
{
    internal class AppointableSeance
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        public DateTimeOffset Time { get; set; }
    }
}