using MongoDB.Bson.Serialization.Attributes;

namespace ParseBackend.Models.Database.Athena
{
    [BsonIgnoreExtraElements]
    public class AthenaData : BaseDatabase
    {
        [BsonElement("items")]
        public List<AthenaItemsData> Items { get; set; }

        [BsonElement("currentLoadOuts")]
        public List<AthenaLoadOuts> CurrentLoadOuts { get; set; }

        [BsonElement("currentLoadOutsList")]
        public List<string> CurrentLoadOutsList { get; set; }

        [BsonElement("stats")]
        public AthenaStatsData Stats { get; set; }

        [BsonElement("dailyQuest")]
        public AthenaDailyQuestData DailyQuestData { get; set; } = new AthenaDailyQuestData();
    }

    public class AthenaDailyQuestData
    {
        [BsonElement("dailyLoginInterval")]
        public DateTime DailyLoginInterval { get; set; }

        [BsonElement("dailyLoginIntervalString")]
        public string DailyLoginIntervalString { get; set; }

        [BsonElement("dailyQuestRerolls")]
        public int DailyQuestRerolls { get; set; }

        [BsonElement("quests")]
        public List<AthenaChallengeData> Quests { get; set; } = new List<AthenaChallengeData>();
    }

    public class AthenaChallengeData
    {
        [BsonElement("itemId")]
        public string ItemId { get; set; }

        [BsonElement("parentAsset")]
        public string ParentAsset { get; set; }

        [BsonElement("objectives")]
        public List<string> Objectives { get; set; }
    }

    public class AthenaStatsData
    {
        [BsonElement("level")]
        public int Level { get; set; }

        [BsonElement("accountLevel")]
        public int AccountLevel { get; set; }

        [BsonElement("xp")]
        public int Xp { get; set; }

        [BsonElement("currentItems")]
        public AthenaCurrentItems CurrentItems { get; set; }

        [BsonElement("battlePassPurchased")]
        public bool BattlePassPurchased { get; set; }

        [BsonElement("battlePassStars")]
        public int BattlePassStars { get; set; }

        [BsonElement("battleBoost")]
        public int BattleBoost { get; set; }

        [BsonElement("battleBoostFriend")]
        public int BattleBoostFriend { get; set; }

        [BsonElement("battlePassTiers")]
        public int BattlePassTiers { get; set; }

        [BsonElement("questAssist")]
        public string QuestAssist { get; set; }
    }

    public class AthenaCurrentItems
    {
        [BsonElement("currentSkin")]
        public string CurrentSkin { get; set; }

        [BsonElement("currentBackbling")]
        public string CurrentBackbling { get; set; }

        [BsonElement("currentPickaxe")]
        public string CurrentPickaxe { get; set; }

        [BsonElement("currentGlider")]
        public string CurrentGlider { get; set; }

        [BsonElement("currentTrail")]
        public string CurrentTrail { get; set; }

        [BsonElement("currentEmotes")]
        public List<string> CurrentEmotes { get; set; }

        [BsonElement("currentWraps")]
        public List<string> CurrentWraps { get; set; }

        [BsonElement("currentMusic")]
        public string CurrentMusic { get; set; }

        [BsonElement("currentLoadingScreen")]
        public string CurrentLoadingScreen { get; set; }
    }

    public class AthenaLoadOuts
    {
        [BsonElement("items")]
        public AthenaCurrentItems Items { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("id")]
        public string Id { get; set; }
    }
}
