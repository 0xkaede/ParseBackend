﻿using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Profile.Attributes
{
    public class CurrencyAttributes
    {
        [JsonProperty("platform")]
        public string Platform { get; set; }
    }
}
