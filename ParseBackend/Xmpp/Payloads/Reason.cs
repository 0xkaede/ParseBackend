using Newtonsoft.Json;

namespace ParseBackend.Xmpp.Payloads
{
    public class Reason
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("reason")]
        public string Reasoning { get; set; }
    }
}
