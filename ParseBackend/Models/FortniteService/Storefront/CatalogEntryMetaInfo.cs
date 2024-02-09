﻿using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Storefront
{
    public class CatalogEntryMetaInfo
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}