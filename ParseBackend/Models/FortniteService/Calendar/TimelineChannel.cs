using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ParseBackend.Models.FortniteService.Calendar
{
    public class TimelineChannel
    {
        [JsonProperty("states")]
        public List<ChannelState> States { get; set; }

        [JsonProperty("cacheExpire")]
        public string CacheExpire { get; set; }
    }
}