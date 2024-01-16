using MongoDB.Bson.Serialization.Attributes;
using ParseBackend.Models.Database.Athena;

namespace ParseBackend.Models.Database.CommonCore
{
    [BsonIgnoreExtraElements]
    public class CommonCoreData : BaseDatabase
    {
        [BsonElement("vbucks")]
        public int Vbucks { get; set; }

        [BsonElement("items")]
        public List<CommonCoreItems> Items { get; set; }

        [BsonElement("gifts")]
        public List<CommonCoreItems> Gifts { get; set; }
    }
}
