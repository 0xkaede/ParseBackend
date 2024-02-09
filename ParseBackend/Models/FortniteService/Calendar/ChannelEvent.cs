using System;
using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Calendar
{
    public class ChannelEvent
    {
        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("activeUntil")]
        public string ActiveUntil { get; set; }

        [JsonProperty("activeSince")]
        public string ActiveSince { get; set; }
    }
}