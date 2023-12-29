using MongoDB.Bson.Serialization.Attributes;

namespace ParseBackend.Models.Database
{
    public class UserData : BaseDatabase
    {
        [BsonElement("createDate")]
        public string AccountId { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("banData")]
        public BanData BannedData { get; set; }
    }

    public class BanData
    {
        [BsonElement("isBanned")]
        public bool IsBanned { get; set; }

        [BsonElement("hours")]
        public int Hours { get; set; }

        [BsonElement("reason")]
        public string Reason { get; set; }
    }
}
