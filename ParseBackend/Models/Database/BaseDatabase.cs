using MongoDB.Bson.Serialization.Attributes;

namespace ParseBackend.Models.Database
{
    public class BaseDatabase
    {
        [BsonElement("accountId")]
        public string AccountId { get; set; }
    }
}
