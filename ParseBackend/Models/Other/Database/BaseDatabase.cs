﻿using MongoDB.Bson.Serialization.Attributes;
using ParseBackend.Models.FortniteService.Profile;

namespace ParseBackend.Models.Other.Database
{
    [BsonIgnoreExtraElements]
    public class BaseDatabase
    {
        [BsonElement("accountId")]
        public string AccountId { get; set; }

        [BsonElement("createdDate")]
        public string Created { get; set; }

        [BsonElement("rvn")]
        public int Rvn { get; set; }
    }
}
