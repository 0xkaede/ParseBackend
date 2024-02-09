using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Content
{
    public class Lobby : BasePagesEntry
    {
        [JsonProperty("stage")]
        public string Stage { get; set; }

        [JsonProperty("backgroundimage")]
        public string BackgroundImage { get; set; }
    }
}