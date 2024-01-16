using MongoDB.Bson.Serialization.Attributes;
using ParseBackend.Models.Profile.Attributes;

namespace ParseBackend.Models.Database.CommonCore
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
