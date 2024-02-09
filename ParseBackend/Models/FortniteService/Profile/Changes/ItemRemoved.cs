using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Profile.Changes
{
    public class ItemRemoved : BaseProfileChange
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        public ItemRemoved()
        {
            ChangeType = "itemRemoved";
        }
    }
}
