using Newtonsoft.Json;

namespace ParseBackend.Xmpp.Payloads
{
    public class Friend
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }
    }
}
