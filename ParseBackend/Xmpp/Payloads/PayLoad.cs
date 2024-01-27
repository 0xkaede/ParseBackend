using Newtonsoft.Json;

namespace ParseBackend.Xmpp.Payloads
{
    public class PayLoad<T>
    {
        [JsonProperty("payload")]
        public T Payload { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
