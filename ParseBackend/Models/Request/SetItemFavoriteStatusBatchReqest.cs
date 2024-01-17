using Newtonsoft.Json;

namespace ParseBackend.Models.Request
{
    public class SetItemFavoriteStatusBatchRequest
    {
        [JsonProperty("itemFavStatus")]
        public bool[] ItemFavStatus { get; set; }

        [JsonProperty("itemIds")]
        public string[] ItemIds { get; set; }
    }
}
