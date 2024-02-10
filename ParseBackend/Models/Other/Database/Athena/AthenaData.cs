using MongoDB.Bson.Serialization.Attributes;
using ParseBackend.Models.FortniteService.Profile;
using ParseBackend.Models.FortniteService;
using ParseBackend.Models.Other.Database;
using Newtonsoft.Json.Linq;
using ParseBackend.Models.FortniteService.Profile.Attributes;
using ParseBackend.Models.FortniteService.Profile.Stats;
using static ParseBackend.Global;
using MongoDB.Driver;
using ParseBackend.Exceptions;
using System;
using ParseBackend.Models.Other.CUE4Parse.Challenges;
using ParseBackend.Models.FortniteService.Profile.Changes;

namespace ParseBackend.Models.Other.Database.Athena
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

        public ProfileResponse CreateMcpResponse(List<object> profileChanges)
        {
            var baseRvn = Rvn;

            Rvn += 1;

            return new ProfileResponse
            {
                ProfileRevision = Rvn,
                ProfileId = "athena",
                ProfileChangesBaseRevisionRevision = baseRvn,
                ProfileChanges = profileChanges ?? new List<object>(),
                ProfileCommandRevision = Rvn,
                ServerTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                ResponseVersion = 1
            };
        }

        public object EquipItem(string itemId, string type, int index)
            => type switch
            {
                "Character" => Stats.CurrentItems.CurrentSkin = itemId,
                "Backpack" => Stats.CurrentItems.CurrentBackbling = itemId,
                "Pickaxe" => Stats.CurrentItems.CurrentPickaxe = itemId,
                "SkyDiveContrail" => Stats.CurrentItems.CurrentTrail = itemId,
                "Glider" => Stats.CurrentItems.CurrentGlider = itemId,
                "MusicPack" => Stats.CurrentItems.CurrentMusic = itemId,
                "LoadingScreen" => Stats.CurrentItems.CurrentLoadingScreen = itemId,
                "Dance" => (Stats.CurrentItems.CurrentEmotes[index] = itemId).ToList(),
                "ItemWrap" => index is -1 ? Stats.CurrentItems.CurrentWraps.Select(x => itemId).ToList() : (Stats.CurrentItems.CurrentWraps[index] = itemId).ToList(),
                _ => throw new BaseException("", $"The item type \"{type}\" was not found!", 1142, "")
            };

        public void UpdateQuests(Dictionary<string, BaseChallenge> challengeData)
        {
            var updateList = new List<AthenaChallengeData>();

            foreach (var challenge in challengeData)
            {
                var data = new AthenaChallengeData
                {
                    ParentAsset = "",
                    Objectives = new List<string>(),
                    ItemId = challenge.Key
                };

                foreach (var chal in challenge.Value.Objects)
                    data.Objectives.Add($"completion_{chal.Key}:0");

                updateList.Add(data);
            }

            DailyQuestData.DailyLoginInterval = DateTime.Now;
            DailyQuestData.Quests = updateList;
        }

        public void CheckAndUpdateRerolls()
        {
            if (DailyQuestData.DailyQuestRerolls <= 0)
                DailyQuestData.DailyQuestRerolls = 1;
        }

        public bool CheckQuestLogin() => DailyQuestData.DailyLoginInterval.AddHours(24) < DateTime.Now;

        public void SeeItem(string itemId)
            => Items.FirstOrDefault(x => x.ItemIdResponse == itemId)!.Seen = true;

        public void FavoriteItem(string itemId, bool status)
            => Items.FirstOrDefault(x => x.ItemIdResponse == itemId)!.IsFavorite = status;

        public Profile CreateFortniteProfile()
        {
            var athena = new Profile
            {
                Created = Created,
                AccountId = AccountId,
                ProfileId = "athena",
                Revision = Rvn, //todo
                WipeNumber = 0,
                CommandRevision = Rvn,
                Updated = CurrentTime(),
                Version = "no_version",
                Items = new Dictionary<string, ProfileItem>(),
                Stats = new ProfileStats
                {
                    Attributes = JObject.FromObject(new AthenaStats
                    {
                        AccountLevel = Stats.AccountLevel,
                        Level = Stats.Level,
                        FavoriteCharacter = Stats.CurrentItems.CurrentSkin,
                        FavoriteBackpack = Stats.CurrentItems.CurrentBackbling,
                        FavoritePickaxe = Stats.CurrentItems.CurrentPickaxe,
                        FavoriteDance = Stats.CurrentItems.CurrentEmotes,
                        FavoriteGlider = Stats.CurrentItems.CurrentGlider,
                        FavoriteItemWraps = Stats.CurrentItems.CurrentWraps,
                        FavoriteLoadingScreen = Stats.CurrentItems.CurrentLoadingScreen,
                        FavoriteMusicPack = Stats.CurrentItems.CurrentMusic,
                        FavoriteSkyDiveContrail = Stats.CurrentItems.CurrentTrail,
                        BookPurchased = Stats.BattlePassPurchased,
                        BookLevel = Stats.BattlePassTiers,
                        BookXp = Stats.BattlePassStars,
                        SeasonMatchBoost = Stats.BattleBoost,
                        SeasonFriendMatchBoost = Stats.BattleBoostFriend,
                        SeasonNum = Config.FortniteSeason,
                        QuestManager = new QuestManager
                        {
                            DailyLoginInterval = DateTime.Now,
                            DailyQuestRerolls = 0
                        },
                        PastSeasons = new List<Season>
                        {
                            new Season
                            {
                            }
                        },
                        PartyAssistQuest = Stats.QuestAssist
                    })
                }
            };

            foreach (var item in Items)
            {
                athena.Items.Add(item.ItemIdResponse, new ProfileItem
                {
                    Attributes = JObject.FromObject(new ItemAttributes
                    {
                        Favorite = item.IsFavorite,
                        ItemSeen = item.Seen,
                        Level = 0,
                        MaxLevelBonus = 0,
                        Variants = item.Variants,
                        XP = 0,
                    }),
                    Quantity = item.Amount,
                    TemplateId = item.ItemId
                });
            }

            foreach (var item in CurrentLoadOuts)
            {
                athena.Items.Add(item.Id, new ProfileItem
                {
                    Attributes = JObject.FromObject(new CosmeticLockerAttributes
                    {
                        LockerName = item.Name,
                        LockerSlotsData = new LockerSlotsData
                        {
                            Slots = new Dictionary<string, Slot>()
                            {
                                {
                                    "Character",
                                    new Slot
                                    {
                                        Items = new List<string> { Stats.CurrentItems.CurrentSkin },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "Backpack",
                                    new Slot
                                    {
                                        Items = new List<string> { Stats.CurrentItems.CurrentBackbling },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "SkyDiveContrail",
                                    new Slot
                                    {
                                        Items = new List<string> { Stats.CurrentItems.CurrentTrail },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "LoadingScreen",
                                    new Slot
                                    {
                                        Items = new List<string> { Stats.CurrentItems.CurrentLoadingScreen },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "Pickaxe",
                                    new Slot
                                    {
                                        Items = new List<string> { Stats.CurrentItems.CurrentPickaxe },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "Glider",
                                    new Slot
                                    {
                                        Items = new List<string> { Stats.CurrentItems.CurrentGlider },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "ItemWrap",
                                    new Slot
                                    {
                                        Items = Stats.CurrentItems.CurrentWraps,
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "Dance",
                                    new Slot
                                    {
                                        Items = Stats.CurrentItems.CurrentEmotes,
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                            }
                        },
                        ItemSeen = true,
                    })
                });
            }

            foreach (var quest in DailyQuestData.Quests)
            {
                var questAttributes = new QuestAttributes
                {
                    ChallengeBundleId = "",
                    ChallengeLinkedQuestGiven = "",
                    ChallengeLinkedQuestParent = "",
                    CreationTime = CurrentTime(),
                    Favorite = false,
                    ItemSeen = false,
                    SentNewNotification = true,
                    LastStateChangeTime = CurrentTime(),
                    QuestState = "Active",
                    MaxLevelBonus = 0,
                    Level = -1,
                    XP = 0,
                    XpRewardScalar = 0,
                    QuestPool = "",
                    QuestRarity = "uncommon",
                };

                var data = JObject.FromObject(questAttributes);

                foreach (var objectives in quest.Objectives)
                {
                    var compSplit = objectives.Split(':');
                    data[$"completion_{compSplit[0]}"] = int.Parse(compSplit[1]);
                }

                athena.Items.Add(quest.ItemId.ComputeSHA256Hash(), new ProfileItem
                {
                    TemplateId = quest.ItemId,
                    Attributes = questAttributes,
                    Quantity = 1
                });
            }

            return athena;
        }
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

        [BsonElement("totalXp")]
        public int TotalXp { get; set; }

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
