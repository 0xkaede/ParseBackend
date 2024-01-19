﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace ParseBackend.Models.Calendar.States
{
    public class ClientMatchmakingState
    {
        [JsonProperty("region")]
        public Dictionary<string, Region> Region { get; set; }
    }

    public class Region
    {
        [JsonProperty("eventFlagsForcedOn")]
        public List<string> EventFlagsForcedOn { get; set; }
        
        [JsonProperty("eventFlagsForcedOff", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> EventFlagsForcedOff { get; set; }
    }
}