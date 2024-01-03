using Newtonsoft.Json;

namespace ParseBackend.Models.Profiles
{
    public class AthenaProfile
    {
        [JsonProperty("_id")]
        public string _Id { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("updated")]
        public string Updated { get; set; }

        [JsonProperty("rvn")]
        public int Revision { get; set; }

        [JsonProperty("wipeNumber")]
        public int WipeNumber { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("profileId")]
        public string ProfileId { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("items")]
        public Dictionary<string, ProfileItem> Items { get; set; }

        [JsonProperty("stats")]
        public AthenaStats Stats { get; set; }

        [JsonProperty("commandRevision")]
        public int CommandRevision { get; set; }
    }

    public class ProfileItem
    {
        [JsonProperty("templateId")]
        public string TemplateId { get; set; }

        [JsonProperty("attributes")]
        public object Attributes { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }

    public class BaseAttributes
    {
        [JsonProperty("max_level_bonus")]
        public int MaxLevelBonus { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("item_seen")]
        public bool ItemSeen { get; set; }

        [JsonProperty("xp")]
        public int XP { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }
    }

    public class CosmeticLockerAttributes
    {
        [JsonProperty("locker_slots_data")]
        public LockerSlotsData LockerSlotsData { get; set; }

        [JsonProperty("use_count")]
        public int UseCount { get; set; }

        [JsonProperty("banner_icon_template")]
        public string BannerIconTemplate { get; set; }

        [JsonProperty("banner_color_template")]
        public string BannerColorTemplate { get; set; }

        [JsonProperty("locker_name")]
        public string LockerName { get; set; }

        [JsonProperty("item_seen")]
        public bool ItemSeen { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }
    }

    public class LockerSlotsData
    {
        [JsonProperty("slots")]
        public Dictionary<string, Slot> Slots { get; set; } = new();
    }

    public class Slot
    {
        [JsonProperty("items")]
        public List<string> Items { get; set; }

        [JsonProperty("activeVariants")]
        public List<object> ActiveVariants { get; set; }
    }

    public class ItemAttributes
    {
        [JsonProperty("variants")]
        public List<Variant> Variants { get; set; }

        [JsonProperty("max_level_bonus")]
        public int MaxLevelBonus { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("item_seen")]
        public bool ItemSeen { get; set; }

        [JsonProperty("xp")]
        public int XP { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }
    }

    public class Variant
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("active")]
        public string Active { get; set; }

        [JsonProperty("owned")]
        public List<string> Owned { get; set; }
    }

    public class AthenaAtrributes
    {
        [JsonProperty("favorite_character", NullValueHandling = NullValueHandling.Ignore)]
        public string FavoriteCharacter { get; set; }

        [JsonProperty("favorite_backpack", NullValueHandling = NullValueHandling.Ignore)]
        public string FavoriteBackpack { get; set; }

        [JsonProperty("favorite_pickaxe", NullValueHandling = NullValueHandling.Ignore)]
        public string FavoritePickaxe { get; set; }

        [JsonProperty("favorite_glider", NullValueHandling = NullValueHandling.Ignore)]
        public string FavoriteGlider { get; set; }

        [JsonProperty("favorite_skydivecontrail", NullValueHandling = NullValueHandling.Ignore)]
        public string FavoriteSkyDiveContrail { get; set; }

        [JsonProperty("favorite_dance", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> FavoriteDance { get; set; }

        [JsonProperty("favorite_itemwraps", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> FavoriteItemWraps { get; set; }

        [JsonProperty("favorite_loadingscreen", NullValueHandling = NullValueHandling.Ignore)]
        public string FavoriteLoadingScreen { get; set; }

        [JsonProperty("favorite_musicpack", NullValueHandling = NullValueHandling.Ignore)]
        public string FavoriteMusicPack { get; set; }

        [JsonProperty("use_random_loadouts")]
        public bool UseRandomLoadouts { get; set; }

        [JsonProperty("banner_icon")]
        public string BannerIcon { get; set; }

        [JsonProperty("banner_color")]
        public string BannerColor { get; set; }

        [JsonProperty("season_match_boost")]
        public int SeasonMatchBoost { get; set; }

        [JsonProperty("loadouts")]
        public List<string> Loadouts { get; set; }

        [JsonProperty("mfa_reward_claimed")]
        public bool MfaRewardClaimed { get; set; }

        [JsonProperty("rested_xp_overflow")]
        public int RestedXpOverflow { get; set; }

        [JsonProperty("quest_manager")]
        public QuestManager QuestManager { get; set; }

        [JsonProperty("book_level")]
        public int BookLevel { get; set; }

        [JsonProperty("season_num")]
        public int SeasonNum { get; set; }

        [JsonProperty("season_update")]
        public int SeasonUpdate { get; set; }

        [JsonProperty("book_xp")]
        public int BookXp { get; set; }

        [JsonProperty("permissions")]
        public List<object> Permissions { get; set; }

        [JsonProperty("battlestars")]
        public int BattleStars { get; set; }

        [JsonProperty("battlestars_season_total")]
        public int BattleStarsSeasonTotal { get; set; }

        [JsonProperty("book_purchased")]
        public bool BookPurchased { get; set; }

        [JsonProperty("lifetime_wins")]
        public int LifetimeWins { get; set; }

        [JsonProperty("party_assist_quest")]
        public string PartyAssistQuest { get; set; }

        [JsonProperty("purchased_battle_pass_tier_offers")]
        public List<PurchasedBattlePassTierOffer> PurchasedBattlePassTierOffers { get; set; }

        [JsonProperty("rested_xp_exchange")]
        public double RestedXpExchange { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("xp_overflow")]
        public int XpOverflow { get; set; }

        [JsonProperty("rested_xp")]
        public int RestedXp { get; set; }

        [JsonProperty("rested_xp_mult")]
        public double RestedXpMult { get; set; }

        [JsonProperty("season_first_tracking_bits")]
        public List<object> SeasonFirstTrackingBits { get; set; }

        [JsonProperty("accountLevel")]
        public int AccountLevel { get; set; }

        [JsonProperty("competitive_identity")]
        public object CompetitiveIdentity { get; set; }

        [JsonProperty("last_applied_loadout")]
        public string LastAppliedLoadout { get; set; }

        [JsonProperty("daily_rewards")]
        public object DailyRewards { get; set; }

        [JsonProperty("xp")]
        public int Xp { get; set; }

        [JsonProperty("season_friend_match_boost")]
        public int SeasonFriendMatchBoost { get; set; }

        [JsonProperty("last_match_end_datetime")]
        public DateTime LastMatchEndDateTime { get; set; }

        [JsonProperty("active_loadout_index")]
        public int ActiveLoadoutIndex { get; set; }

        [JsonProperty("inventory_limit_bonus")]
        public int InventoryLimitBonus { get; set; }
    }

    public class AthenaStats
    {
        [JsonProperty("attributes")]
        public AthenaAtrributes Attributes { get; set; }
    }

    public class PurchasedBattlePassTierOffer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class QuestManager
    {
        [JsonProperty("dailyLoginInterval")]
        public DateTime DailyLoginInterval { get; set; }

        [JsonProperty("dailyQuestRerolls")]
        public int DailyQuestRerolls { get; set; }
    }

    public class Vote
    {
        [JsonProperty("voteCount")]
        public int VoteCount { get; set; }

        [JsonProperty("firstVoteAt")]
        public DateTime FirstVoteAt { get; set; }

        [JsonProperty("lastVoteAt")]
        public DateTime LastVoteAt { get; set; }
    }
}
