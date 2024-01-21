using Newtonsoft.Json;

namespace ParseBackend.Models.Request
{
    public class PurchaseCatalogEntryRequest
    {
        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("purchaseQuantity")]
        public int PurchaseQuantity { get; set; }

        [JsonProperty("currencySubType")]
        public string CurrencySubType { get; set; }

        [JsonProperty("expectedTotalPrice")]
        public int ExpectedTotalPrice { get; set; }

        [JsonProperty("gameContext")]
        public string GameContext { get; set; }
    }
}
