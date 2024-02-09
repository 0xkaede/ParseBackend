using MongoDB.Bson.Serialization.Attributes;

namespace ParseBackend.Models.Other.Database.Other
{
    [BsonIgnoreExtraElements]
    public class MtxAffiliateData
    {
        [BsonElement("accountId")]
        public string AccountId { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("vbucksSpent")]
        public int VbuckSpent { get; set; }

        [BsonElement("purchaces")]
        public int Purchaces { get; set; }
    }
}
