using MongoDB.Bson.Serialization.Attributes;
using ParseBackend.Enums;

namespace ParseBackend.Models.Database.Other
{
    [BsonIgnoreExtraElements]
    public class FriendsData
    {
        [BsonElement("accountId")]
        public string AccountId { get; set; }

        [BsonElement("list")]
        public List<FriendsListData> List { get; set; }
    }

    public class FriendsListData
    {
        [BsonElement("created")]
        public DateTime Created { get; set; }

        [BsonElement("accountId")]
        public string AccountId { get; set; }

        [BsonElement("status")]
        public FriendsStatus Status { get; set; }
    }
}
