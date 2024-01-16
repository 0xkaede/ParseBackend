using Newtonsoft.Json;

namespace ParseBackend.Models.Request
{
    public class EquipBattleRoyaleCustomizationRequest
    {
        [JsonProperty("slotName")]
        public string SlotName { get; set; }

        [JsonProperty("itemToSlot")]
        public string ItemToSlot { get; set; }

        [JsonProperty("indexWithinSlot")]
        public int IndexWithinSlot { get; set; }

        [JsonProperty("variantUpdates")]
        public List<EquipBattleRoyaleCustomizationStylesRequest> VariantUpdates { get; set; }
    }

    public class EquipBattleRoyaleCustomizationStylesRequest
    {
        [JsonProperty("active")]
        public string Active { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }
    }
}
