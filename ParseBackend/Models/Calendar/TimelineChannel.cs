using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ParseBackend.Models.Calendar
{
    public class TimelineChannel
    {
        [JsonProperty("states")]
        public List<ChannelState> States { get; set; }

        [JsonProperty("cacheExpire")]
        public string CacheExpire { get; set; }
    }
}