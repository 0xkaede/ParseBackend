using Newtonsoft.Json;
using ParseBackend.Exceptions;

namespace ParseBackend.Models.FortniteService.Request
{
    public class GiftCatalogEntryRequest
    {
        [JsonProperty("offerId")]
        public string OfferId { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("currencySubType")]
        public string CurrencySubType { get; set; }

        [JsonProperty("expectedTotalPrice")]
        public int ExpectedTotalPrice { get; set; }

        [JsonProperty("gameContext")]
        public string GameContext { get; set; }

        [JsonProperty("receiverAccountIds")]
        public List<string> ReceiverAccountIds { get; set; }

        [JsonProperty("giftWrapTemplateId")]
        public string GiftWrapTemplateId { get; set; }

        [JsonProperty("personalMessage")]
        public string PersonalMessage { get; set; }

        public void ValidateRequest(int remaining, bool isGifting = false)
        {
            if (isGifting)
            {
                var giftBoxes = new List<string>
                {
                    "GiftBox:gb_default",
                    "GiftBox:gb_giftwrap1",
                    "GiftBox:gb_giftwrap2",
                    "GiftBox:gb_giftwrap3",
                };

                if (PersonalMessage.Length > 100)
                    throw new BaseException("errors.com.epicgames.length", "The message can be no longer then 100 characters!", 19734, "");

                if (!giftBoxes.Contains(GiftWrapTemplateId))
                    throw new BaseException("errors.com.epicgames.giftbox_invalid", "Please select a valid giftbox!", 14394, "");

                if (ReceiverAccountIds.Count < 1 || ReceiverAccountIds.Count > remaining)
                    throw new BaseException("errors.com.epicgames.gift_amount.users", $"You have to atleast gift 1 friends and you cant gift more then {remaining} friends today!", 14394, "");

                if (ReceiverAccountIds.Count != ReceiverAccountIds.Distinct().Count())
                    throw new BaseException("errors.com.epicgames.duped_users", "You cannot gift a user more then once at a time!", 15632, "");
            }
        }
    }
}
