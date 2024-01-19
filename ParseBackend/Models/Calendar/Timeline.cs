using Newtonsoft.Json;

namespace ParseBackend.Models.Calendar
{
    public class Timeline
    {
        [JsonProperty("channels")]
        public Dictionary<string, TimelineChannel> Channels { get; set; }

        [JsonProperty("currentTime")]
        public string CurrentTime { get; set; }

        [JsonProperty("cacheIntervalMins")]
        public double CacheIntervalMinutes { get; set; }
    }

}
