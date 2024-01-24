using MongoDB.Bson.Serialization.Attributes;

namespace ParseBackend.Models.Database.Other
{
    [BsonIgnoreExtraElements]
    public class ExchangeCode
    {
        [BsonElement("accountId")]
        public string AccountId { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("dateCreated")]
        public DateTime DateCreated { get; set; }
    }
}
