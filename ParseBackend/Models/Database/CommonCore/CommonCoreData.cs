using MongoDB.Bson.Serialization.Attributes;
using ParseBackend.Models.Database.Athena;

namespace ParseBackend.Models.Database.CommonCore
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
