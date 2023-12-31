﻿using MongoDB.Bson.Serialization.Attributes;
using ParseBackend.Models.Profiles;

namespace ParseBackend.Models.Database
{
    public class AthenaItemsData
    {
        [BsonElement("itemId")]
        public string ItemId { get; set; }

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
