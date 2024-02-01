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
        public List<CommonCoreItems> Gifts { get; set; }

        [BsonElement("vbucks")]
        public int Vbucks { get; set; }

        [BsonElement("stats")]
        public CommonCoreDataStats Stats { get; set; }
    }

    public class CommonCoreDataStats
    {
        [BsonElement("mtxAffiliate")]
        public string MtxAffiliate { get; set; }

        [BsonElement("mtxAffiliateTime")]
        public DateTime MtxAffiliateTime { get; set; }
    }
}
