using MongoDB.Bson.Serialization.Attributes;

namespace ParseBackend.Models.Other.Database.CommonCore
{
    public class CommonCoreItems
    {
        [BsonElement("itemId")]
        public string ItemId { get; set; }

        [BsonElement("isFavorite")]
        public bool IsFavorite { get; set; }

        [BsonElement("seen")]
        public bool Seen { get; set; }

        [BsonElement("amount")]
        public int Amount { get; set; }
    }
}
