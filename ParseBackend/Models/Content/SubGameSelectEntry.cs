using Newtonsoft.Json;

namespace ParseBackend.Models.Content
{
    public class SubGameSelectEntry : BasePagesEntry
    {
        [JsonProperty("battleRoyale")]
        public MessageEntry BattleRoyale { get; set; }

        [JsonProperty("creative")]
        public MessageEntry Creative { get; set; }

        [JsonProperty("saveTheWorld")]
        public MessageEntry SaveTheWorld { get; set; }

        [JsonProperty("saveTheWorldUnowned")] 
        public MessageEntry SaveTheWorldUnowned { get; set; }
    }
}