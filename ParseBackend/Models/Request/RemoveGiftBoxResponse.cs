using Newtonsoft.Json;

namespace ParseBackend.Models.Request
{
    public class RemoveGiftBoxResponse
    {
        [JsonProperty("giftBoxItemId")]
        public string GiftBoxItemIds { get; set; }
    }
}
