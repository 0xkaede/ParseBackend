using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Request
{
    public class MarkItemSeenRequest
    {
        [JsonProperty("itemIds")]
        public string[] ItemIds { get; set; }
    }
}
