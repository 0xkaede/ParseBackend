using Newtonsoft.Json;

namespace ParseBackend.Models.Request
{
    public class MarkItemSeenRequest
    {
        [JsonProperty("itemIds")]
        public string[] ItemIds { get; set; }
    }
}
