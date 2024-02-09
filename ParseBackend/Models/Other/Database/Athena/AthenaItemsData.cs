using MongoDB.Bson.Serialization.Attributes;
using ParseBackend.Models.FortniteService.Profile.Attributes;
using static ParseBackend.Global;

namespace ParseBackend.Models.Other.Database.Athena
{
    public class AthenaItemsData
    {
        [BsonElement("itemId")]
        public string ItemId { get; set; }

        [BsonElement("itemIdResponse")]
        public string ItemIdResponse { get; set; }

        [BsonElement("variant")]
        public List<Variant> Variants { get; set; }

        [BsonElement("isFavorite")]
        public bool IsFavorite { get; set; }

        [BsonElement("seen")]
        public bool Seen { get; set; }

        [BsonElement("amount")]
        public int Amount { get; set; }
    }
}
