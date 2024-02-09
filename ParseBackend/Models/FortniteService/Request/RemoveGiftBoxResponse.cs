using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Request
{
    public class RemoveGiftBoxResponse
    {
        [JsonProperty("giftBoxItemId")]
        public string GiftBoxItemIds { get; set; }
    }
}
