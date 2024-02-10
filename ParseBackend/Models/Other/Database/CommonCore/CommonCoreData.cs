using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using ParseBackend.Exceptions;
using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Profile;
using ParseBackend.Models.FortniteService.Profile.Attributes;
using ParseBackend.Models.FortniteService.Profile.Stats;
using ParseBackend.Models.FortniteService.Storefront;
using ParseBackend.Utils;
using static ParseBackend.Global;

namespace ParseBackend.Models.Other.Database.CommonCore
{
    [BsonIgnoreExtraElements]
    public class CommonCoreData : BaseDatabase
    {
        [BsonElement("items")]
        public List<CommonCoreItems> Items { get; set; }

        [BsonElement("gifts")]
        public List<CommonCoreDataGifts> Gifts { get; set; }

        [BsonElement("vbucks")]
        public int Vbucks { get; set; }

        [BsonElement("stats")]
        public CommonCoreDataStats Stats { get; set; }

        public List<object> PurchaceCatalog(CatalogEntry catalog, int usersAmount = 1)
        {
            var profileChanges = new List<object>();

            if (catalog.IsMxtCurrency())
            {
                bool bHasPaid = false;
                var price = catalog.GetPrice(usersAmount);

                if (Vbucks < price)
                    throw new BaseException("errors.com.epicgames.currency.mtx.insufficient",
                    $"You can not afford this item ({price}), you only have {Vbucks}.", 1458, "");

                Vbucks -= price;

                profileChanges.Add(JObject.FromObject(new //being lazy
                {
                    changeType = "itemQuantityChanged",
                    itemId = "Currency:MtxPurchased".ComputeSHA256Hash(),
                    quantity = Vbucks
                }));

                if (!bHasPaid && price > 0)
                    throw new BaseException("errors.com.epicgames.currency.mtx.insufficient", $"You can not afford this item ({price})", 4735, "");
            }

            return profileChanges;
        }

        public ProfileResponse CreateMcpResponse(List<object> profileChanges)
        {
            var baseRvn = Rvn;

            Rvn += 1;

            return new ProfileResponse
            {
                ProfileRevision = Rvn,
                ProfileId = "common_core",
                ProfileChangesBaseRevisionRevision = baseRvn,
                ProfileChanges = profileChanges ?? new List<object>(),
                ProfileCommandRevision = Rvn,
                ServerTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                ResponseVersion = 1
            };
        }

        public Profile CreateFortniteProfile(UserData userData)
        {
            var commonCore = new Profile
            {
                Created = Created,
                AccountId = AccountId,
                ProfileId = "common_core",
                Revision = Rvn, //todo
                WipeNumber = 0,
                CommandRevision = Rvn,
                Updated = CurrentTime(),
                Version = "no_version",
                Items = new Dictionary<string, ProfileItem>(),
                Stats = new ProfileStats
                {
                    Attributes = JObject.FromObject(new CommonCoreStats
                    {
                        MtxPurchaseHistory = new MtxPurchaseHistory
                        {
                            RefundsUsed = 0,
                            RefundCredits = 3,
                            Purchases = new List<MtxPurchase>()
                            {

                            }
                        },
                        MfaEnabled = true,
                        MtxAffiliate = "",
                        CurrentMtxPlatform = "EpicPC",
                        AllowedToReceiveGifts = true,
                        AllowedToSendGifts = true,
                        GiftHistory = new GiftHistory(),
                        BanStatus = new BanStatus
                        {
                            RequiresUserAck = userData.BannedData.Type is Enums.BannedType.MatchMaking ? true : false,
                            BanReasons = new List<string> { userData.BannedData.Reason.GetDescription() },
                            BanHasStarted = userData.BannedData.Type is Enums.BannedType.MatchMaking ? true : false,
                            BanStartTime = userData.BannedData.DateBanned,
                            BanDurationDays = userData.BannedData.Days,
                            AdditionalInfo = "",
                            CompetitiveBanReason = "None",
                            ExploitProgramName = ""
                        },
                        MtxAffiliateId = Stats.MtxAffiliate,
                        MtxAffiliateSetTime = Stats.MtxAffiliateTime
                    })
                }
            };

            commonCore.Items.Add("Currency:MtxPurchased".ComputeSHA256Hash(), new ProfileItem
            {
                Attributes = JObject.FromObject(new CurrencyAttributes
                {
                    Platform = "EpicPC"
                }),
                Quantity = Vbucks,
                TemplateId = "Currency:MtxPurchased"
            });

            if (Gifts.Count != 0)
            {
                var gifts = Gifts;
                foreach (var gift in gifts)
                {
                    var loot = new List<GiftBoxLootList>();

                    foreach (var item in gift.LootList)
                    {
                        loot.Add(new GiftBoxLootList
                        {
                            ItemType = item,
                            ItemGuid = item.ComputeSHA256Hash(),
                            ItemProfile = "athena",
                            Quantity = 1
                        });
                    }

                    commonCore.Items.Add(gift.TemplateIdHashed, new ProfileItem
                    {
                        TemplateId = gift.TemplateId,
                        Attributes = new GiftBoxAttribute
                        {
                            FromAccountId = gift.FromAccountId,
                            GiftedOn = gift.Time.TimeToString(),
                            Level = 1,
                            LootList = loot,
                            Params = new Dictionary<string, string>
                                {
                                    {
                                        "userMessage",
                                        gift.UserMessage
                                    }
                                }
                        },
                        Quantity = 1
                    });
                }
            }

            return commonCore;
        }
    }

    public class CommonCoreDataGifts
    {
        [BsonElement("templateId")]
        public string TemplateId { get; set; }

        [BsonElement("templateIdHashed")]
        public string TemplateIdHashed { get; set; }

        [BsonElement("fromAccountId")]
        public string FromAccountId { get; set; }

        [BsonElement("userMessage")]
        public string UserMessage { get; set; }

        [BsonElement("time")]
        public DateTime Time { get; set; }

        [BsonElement("lootList")]
        public List<string> LootList { get; set; }
    }

    public class CommonCoreDataStats
    {
        [BsonElement("mtxAffiliate")]
        public string MtxAffiliate { get; set; }

        [BsonElement("mtxAffiliateTime")]
        public DateTime MtxAffiliateTime { get; set; }

        [BsonElement("reciveGifts")]
        public bool ReciveGifts { get; set; }

        [BsonElement("giftRemaining")]
        public int GiftRemaining { get; set; }

        [BsonElement("lastGiftRefresh")]
        public DateTime LastGiftRefresh { get; set; }
    }
}
