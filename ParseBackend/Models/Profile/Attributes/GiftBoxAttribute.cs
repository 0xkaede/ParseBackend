using Newtonsoft.Json;

namespace ParseBackend.Models.Profile.Attributes
{
    public class GiftBoxAttribute
    {
        [JsonProperty("fromAccountId")]
        public string FromAccountId { get; set;}

        [JsonProperty("lootList")]
        public List<GiftBoxLootList> LootList { get; set; }

        [JsonProperty("params")]
        public Dictionary<string, string> Params { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("giftedOn")]
        public string GiftedOn { get; set; }
    }

    public class GiftBoxLootList
    {
        [JsonProperty("itemType")]
        public string ItemType { get; set; }

        [JsonProperty("itemGuid")]
        public string ItemGuid { get; set; }

        [JsonProperty("itemProfile")]
        public string ItemProfile { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}
