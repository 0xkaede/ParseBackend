using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Content
{
    public class EmergencyNotice : BasePagesEntry
    {
        [JsonProperty("news")]
        public BattleRoyaleNews News { get; set; }

        [JsonProperty("alwaysShow")]
        public bool AlwaysShow => false;

        [JsonProperty("_noIndex")]
        public bool NoIndex => false;
    }
}