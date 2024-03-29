﻿using Newtonsoft.Json;

namespace ParseBackend.Models.AccountService
{
    public class AccountInfoPublic
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("externalAuths")]
        public List<string> ExternalAuths { get; set; } = new List<string>();
    }
}
