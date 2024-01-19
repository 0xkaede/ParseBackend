using Newtonsoft.Json;

namespace ParseBackend.Models.Profile.Changes
{
    public class ItemAdded : BaseProfileChange
    {
        [JsonProperty("item")]
        public ProfileItem Item { get; set; }

        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        public ItemAdded() { ChangeType = "itemAdded"; }
    }
}
