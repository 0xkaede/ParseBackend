using MongoDB.Bson.Serialization.Attributes;
using ParseBackend.Enums;

namespace ParseBackend.Models.Other.Database
{
    [BsonIgnoreExtraElements]
    public class UserData : BaseDatabase
    {
        [BsonElement("discordId")]
        public string DiscordId { get; set; }

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

        [BsonElement("days")]
        public float Days { get; set; }

        [BsonElement("dateBanned")]
        public string DateBanned { get; set; }

        [BsonElement("reason")]
        public BannedReason Reason { get; set; }

        [BsonElement("type")]
        public BannedType Type { get; set; }
    }
}
