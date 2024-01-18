using Amazon.Runtime.Internal.Transform;
using CUE4Parse.Utils;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json.Linq;
using ParseBackend.Exceptions;
using ParseBackend.Exceptions.Common;
using ParseBackend.Models.Database;
using ParseBackend.Models.Database.Athena;
using ParseBackend.Models.Database.CommonCore;
using ParseBackend.Models.Profile;
using ParseBackend.Models.Profile.Attributes;
using ParseBackend.Models.Profile.Stats;
using ParseBackend.Utils;
using System;
using System.Diagnostics;
using static ParseBackend.Global;

namespace ParseBackend.Services
{
    public interface IMongoService
    {
        public Task<UserData> LoginAccount(string email, string password);

        public Task<UserData> FindUserByAccountId(string accountId);
        public Task<AthenaData> FindAthenaByAccountId(string accountId);

        public Task<Profile> CreateAthenaProfile(string accountId);
        public Task<Profile> CreateCommonCoreProfile(string accountId);
        public Task<Profile> CreateCommonPublicProfile(string accountId);

        public void SeenAthenaItem(ref AthenaData athenaData, string templateId, bool isSeen);
        public void EquipAthenaItem(ref AthenaData athenaData, string itemType, string itemId, int index);
        public void FavoriteAthenaItem(ref AthenaData athenaData, string templateId, bool isFavorite);

        public void UpdateAthenaRvn(ref AthenaData athenaData);
    }

    public class MongoService : IMongoService
    {
        private readonly IFileProviderService _fileProviderService;
        private readonly IMongoCollection<UserData> _userProfiles;
        private readonly IMongoCollection<AthenaData> _athenaData;
        private readonly IMongoCollection<CommonCoreData> _commonCoreData;

        public MongoService(IFileProviderService fileProviderService)
        {
            _fileProviderService = fileProviderService;

            var client = new MongoClient("mongodb://localhost");

            var mongoDatabase = client.GetDatabase("KaedeBackend");

            _userProfiles = mongoDatabase.GetCollection<UserData>("UserProfiles");
            _athenaData = mongoDatabase.GetCollection<AthenaData>("AthenaData");
            _commonCoreData = mongoDatabase.GetCollection<CommonCoreData>("CommonCoreData");

            _ = InitDatabase();
        }

        public async Task InitDatabase()
        {
            Logger.Log("Database Is Online");
            GrantAthenaFullLocker("6567c5ccddde4b91831771cf3e4f333a");
            await CreateAccount("kaede@fn.dev", "kaede1234", "Kaede");
        }

        private FilterDefinition<AthenaData> FilterAthenaItem(string accountId, string templateId)
            => Builders<AthenaData>.Filter.Eq(x => x.AccountId, accountId)
            & Builders<AthenaData>.Filter.ElemMatch(x => x.Items, Builders<AthenaItemsData>.Filter.Eq(x => x.ItemIdResponse, templateId)); // so much better lad

        #region DatabaseFinders

        private async Task<List<UserData>> GetAllUserProfiles()
        {
            var item = await _userProfiles.FindAsync(x => true);
            return item.ToList();
        }

        private async Task<List<AthenaData>> GetAllUserAthenaProfiles()
        {
            var item = await _athenaData.FindAsync(x => true);
            return item.ToList();
        }

        private async Task<List<CommonCoreData>> GetAllUserCommonCoreProfiles()
        {
            var item = await _commonCoreData.FindAsync(x => true);
            return item.ToList();
        }

        public async Task<UserData> FindUserByAccountId(string accountId)
        {
            var users = await GetAllUserProfiles();
            return users.FirstOrDefault(x => x.AccountId == accountId);
        }

        public async Task<AthenaData> FindAthenaByAccountId(string accountId)
        {
            var users = await GetAllUserAthenaProfiles();
            return users.FirstOrDefault(x => x.AccountId == accountId);
        }

        public async Task<CommonCoreData> FindCommonCoreByAccountId(string accountId)
        {
            var users = await GetAllUserCommonCoreProfiles();
            return users.FirstOrDefault(x => x.AccountId == accountId);
        }

        #endregion

        #region DatabaseAccounts

        public async Task CreateAccount(string email, string password, string username)
        {
            var users = await GetAllUserProfiles();

            var usernameCheck = users.FirstOrDefault(x => x.Username == username);
            if (usernameCheck != null)
                throw new UsernameTakenException();

            var emailChekc = users.FirstOrDefault(x => x.Email == email);
            if (emailChekc != null)
                throw new BaseException("", "This Email has already been taken", 108, "");

            var id = CreateUuid();
            var time = CurrentTime();

            var userData = new UserData
            {
                AccountId = id,
                Created = time,
                Email = email,
                Password = password.ComputeSHA256Hash(),
                Username = username,
                BannedData = new BanData
                {
                    IsBanned = false,
                    Type = Enums.BannedType.None,
                    Reason = Enums.BannedReason.Exploiting,
                    Days = 0,
                    DateBanned = CurrentTime()
                }
            };

            var athenaData = new AthenaData
            {
                AccountId = id,
                Created = time,
                Rvn = 0,
                Items = new List<AthenaItemsData>(),
                Stats = new AthenaStatsData
                {
                    AccountLevel = 1,
                    BattlePassPurchased = false,
                    BattlePassStars = 0,
                    BattlePassTiers = 0,
                    Level = 1,
                    Xp = 0,
                    CurrentItems = new AthenaCurrentItems
                    {
                        CurrentSkin = "",
                        CurrentBackbling = "",
                        CurrentPickaxe = "AthenaPickaxe:DefaultPickaxe",
                        CurrentGlider = "AthenaGlider:DefaultGlider",
                        CurrentEmotes = new List<string>() { "", "", "", "", "", "" },
                        CurrentWraps = new List<string>() { "", "", "", "", "", "", "" },
                        CurrentLoadingScreen = "",
                        CurrentMusic = "",
                        CurrentTrail = "",
                    },
                    BattleBoost = 0,
                    BattleBoostFriend = 0,
                },
                CurrentLoadOuts = new List<AthenaLoadOuts> {
                        new AthenaLoadOuts
                        {
                            Id = "SandBox_LoadOut",
                            Name = "Parse Backend",
                            Items = new AthenaCurrentItems
                            {
                                CurrentSkin = "",
                                CurrentBackbling = "",
                                CurrentPickaxe = "AthenaPickaxe:DefaultPickaxe",
                                CurrentGlider = "AthenaGlider:DefaultGlider",
                                CurrentEmotes = new List<string>() { "", "", "", "", "", "" },
                                CurrentWraps = new List<string>() { "", "", "", "", "", "", "" },
                                CurrentLoadingScreen = "",
                                CurrentMusic = "",
                                CurrentTrail = "",
                            }
                        }
                    },
                CurrentLoadOutsList = new List<string>
                    {
                        "SandBox_LoadOut",
                    }
            };

            var items = new List<string>
            {
                 "AthenaCharacter:CID_001_Athena_Commando_F_Default", "AthenaCharacter:CID_002_Athena_Commando_F_Default", "AthenaCharacter:CID_003_Athena_Commando_F_Default", "AthenaCharacter:CID_004_Athena_Commando_F_Default", "AthenaCharacter:CID_005_Athena_Commando_M_Default", "AthenaCharacter:CID_006_Athena_Commando_M_Default", "AthenaCharacter:CID_007_Athena_Commando_M_Default", "AthenaCharacter:CID_008_Athena_Commando_M_Default", "AthenaCharacter:CID_556_Athena_Commando_F_RebirthDefaultA", "AthenaCharacter:CID_557_Athena_Commando_F_RebirthDefaultB", "AthenaCharacter:CID_558_Athena_Commando_F_RebirthDefaultC", "AthenaCharacter:CID_559_Athena_Commando_F_RebirthDefaultD", "AthenaCharacter:CID_560_Athena_Commando_M_RebirthDefaultA", "AthenaCharacter:CID_561_Athena_Commando_M_RebirthDefaultB", "AthenaCharacter:CID_562_Athena_Commando_M_RebirthDefaultC", "AthenaCharacter:CID_563_Athena_Commando_M_RebirthDefaultD", "AthenaCharacter:cid_a_272_athena_commando_f_prime", "AthenaPickaxe:DefaultPickaxe", "AthenaGlider:DefaultGlider", "AthenaDance:EID_DanceMoves", "AthenaDance:EID_BoogieDown"
            };

            foreach (var item in items)
            {
                try
                {
                    athenaData.Items.Add(new AthenaItemsData
                    {
                        Amount = 1,
                        Seen = false,
                        IsFavorite = false,
                        ItemId = item,
                        ItemIdResponse = item.ComputeSHA256Hash(),
                    });
                }
                catch
                {

                }
            }

            var commonCoreData = new CommonCoreData
            {
                AccountId = id,
                Created = time,
                Gifts = new List<CommonCoreItems>(),
                Items = new List<CommonCoreItems>(),
                Rvn = 0,
                Vbucks = 0
            };

            await _athenaData.InsertOneAsync(athenaData);
            await _commonCoreData.InsertOneAsync(commonCoreData);
            await _userProfiles.InsertOneAsync(userData);

            Logger.Log($"Account Created: {username}");
        }

        public async Task<UserData> LoginAccount(string email, string password)
        {
            var users = await GetAllUserProfiles();
            Logger.Log("test1");
            var emailCheck = users.FirstOrDefault(x => x.Email == email);
            if (email is null)
                throw new BaseException("", "Email wasnt found", 1008, "");
            Logger.Log("test2");
            if (emailCheck.Password != password.ComputeSHA256Hash())
                throw new BaseException("", "Password is wrong, Please try again!", 1008, "");
            Logger.Log("test4");
            return emailCheck;
        }

        public void GrantAthenaFullLocker(string accountId)
        {
            var itemsToGrant = new List<AthenaItemsData>();
            var itemsFromFile = _fileProviderService.GetAllCosmetics();

            foreach (var item in itemsFromFile)
            {
                var itemToLower = item.ToLower();
                var itemRaw = item.SubstringAfterLast("/").SubstringBefore(".");

                var itemFixed = FixCosmetic(itemRaw, itemToLower);
                if (itemFixed == null) continue;

                itemsToGrant.Add(new AthenaItemsData
                {
                    Amount = 1,
                    Seen = false,
                    IsFavorite = false,
                    ItemId = itemFixed,
                    ItemIdResponse = itemFixed.ComputeSHA256Hash(),
                });
            }

            var filter = Builders<AthenaData>.Filter.Eq(x => x.AccountId, accountId);
            var update = Builders<AthenaData>.Update.Set(x => x.Items, itemsToGrant);

            _athenaData.UpdateOne(filter, update);

            string FixCosmetic(string itemRaw, string toLowerItem)
            {
                if (toLowerItem.Contains("characters"))
                    return $"AthenaCharacter:{itemRaw}";

                if (toLowerItem.Contains("backpacks"))
                    return $"AthenaBackpack:{itemRaw}";

                if (toLowerItem.Contains("pickaxe"))
                    return $"AthenaPickaxe:{itemRaw}";

                if (toLowerItem.Contains("dance"))
                    return $"AthenaDance:{itemRaw}";

                if (toLowerItem.Contains("musicpacks"))
                    return $"AthenaMusicPack:{itemRaw}";

                return null;
            }
        }

        #endregion

        #region FortniteProfileCreate

        public async Task<Profile> CreateAthenaProfile(string accountId)
        {
            var athenaData = await FindAthenaByAccountId(accountId);

            var athena = new Profile
            {
                Created = athenaData.Created,
                AccountId = accountId,
                ProfileId = "athena",
                Revision = athenaData.Rvn, //todo
                WipeNumber = 0,
                CommandRevision = athenaData.Rvn,
                Updated = CurrentTime(),
                Version = "no_version",
                Items = new Dictionary<string, ProfileItem>(),
                Stats = new ProfileStats
                {
                    Attributes = JObject.FromObject(new AthenaStats
                    {
                        AccountLevel = athenaData.Stats.AccountLevel,
                        Level = athenaData.Stats.Level,
                        FavoriteCharacter = athenaData.Stats.CurrentItems.CurrentSkin,
                        FavoriteBackpack = athenaData.Stats.CurrentItems.CurrentBackbling,
                        FavoritePickaxe = athenaData.Stats.CurrentItems.CurrentPickaxe,
                        FavoriteDance = athenaData.Stats.CurrentItems.CurrentEmotes,
                        FavoriteGlider = athenaData.Stats.CurrentItems.CurrentGlider,
                        FavoriteItemWraps = athenaData.Stats.CurrentItems.CurrentWraps,
                        FavoriteLoadingScreen = athenaData.Stats.CurrentItems.CurrentLoadingScreen,
                        FavoriteMusicPack = athenaData.Stats.CurrentItems.CurrentMusic,
                        FavoriteSkyDiveContrail = athenaData.Stats.CurrentItems.CurrentTrail,
                        BookPurchased = athenaData.Stats.BattlePassPurchased,
                        BookLevel = athenaData.Stats.BattlePassTiers,
                        BookXp = athenaData.Stats.BattlePassStars,
                        SeasonMatchBoost = athenaData.Stats.BattleBoost,
                        SeasonFriendMatchBoost = athenaData.Stats.BattleBoostFriend,
                        SeasonNum = Config.FortniteSeason
                    })
                }
            };

            //add full locker like elixir

            foreach(var item in athenaData.Items)
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

            foreach (var item in athenaData.CurrentLoadOuts)
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
                                        Items = new List<string> { athenaData.Stats.CurrentItems.CurrentSkin },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "Backpack",
                                    new Slot
                                    {
                                        Items = new List<string> { athenaData.Stats.CurrentItems.CurrentBackbling },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "SkyDiveContrail",
                                    new Slot
                                    {
                                        Items = new List<string> { athenaData.Stats.CurrentItems.CurrentTrail },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "LoadingScreen",
                                    new Slot
                                    {
                                        Items = new List<string> { athenaData.Stats.CurrentItems.CurrentLoadingScreen },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "Pickaxe",
                                    new Slot
                                    {
                                        Items = new List<string> { athenaData.Stats.CurrentItems.CurrentPickaxe },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "Glider",
                                    new Slot
                                    {
                                        Items = new List<string> { athenaData.Stats.CurrentItems.CurrentGlider },
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "ItemWrap",
                                    new Slot
                                    {
                                        Items = athenaData.Stats.CurrentItems.CurrentWraps,
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                                {
                                    "Dance",
                                    new Slot
                                    {
                                        Items = athenaData.Stats.CurrentItems.CurrentEmotes,
                                        ActiveVariants = new List<object> ()
                                    }
                                },
                            }
                        },
                        ItemSeen = true,
                    })
                });
            }

            return athena;
        }

        public async Task<Profile> CreateCommonPublicProfile(string accountId)
        {
            var commonPublic = new Profile
            {
                Created = CurrentTime(),
                AccountId = accountId,
                ProfileId = "common_public",
                Revision = 0, 
                WipeNumber = 0,
                CommandRevision = 0,
                Updated = CurrentTime(),
                Version = "no_version",
                Stats = new ProfileStats(),
                Items = new Dictionary<string, ProfileItem>(),
            };

            return commonPublic;
        }

        public async Task<Profile> CreateCommonCoreProfile(string accountId)
        {
            try
            {
                var userData = await FindUserByAccountId(accountId);
                var commonCoreData = await FindCommonCoreByAccountId(accountId);

                var commonCore = new Profile
                {
                    Created = commonCoreData.Created,
                    AccountId = accountId,
                    ProfileId = "common_core",
                    Revision = commonCoreData.Rvn, //todo
                    WipeNumber = 0,
                    CommandRevision = commonCoreData.Rvn,
                    Updated = CurrentTime(),
                    Version = "no_version",
                    Items = new Dictionary<string, ProfileItem>(),
                    Stats = new ProfileStats
                    {
                        Attributes = JObject.FromObject(new CommonCoreStats
                        {
                            MtxPurchaseHistory = new MtxPurchaseHistory
                            {
                                RefundsUsed = 0,
                                RefundCredits = 3,
                                Purchases = new List<MtxPurchase>()
                            },
                            MfaEnabled = true,
                            MtxAffiliate = "",
                            CurrentMtxPlatform = "EpicPC",
                            AllowedToReceiveGifts = true,
                            AllowedToSendGifts = true,
                            GiftHistory = new GiftHistory(),
                            BanStatus = new BanStatus
                            {
                                RequiresUserAck = userData.BannedData.Type is Enums.BannedType.MatchMaking ? true : false,
                                BanReasons = new List<string> { userData.BannedData.Reason.GetDescription() },
                                BanHasStarted = userData.BannedData.Type is Enums.BannedType.MatchMaking ? true : false,
                                BanStartTime = userData.BannedData.DateBanned,
                                BanDurationDays = userData.BannedData.Days,
                                AdditionalInfo = "",
                                CompetitiveBanReason = "None",
                                ExploitProgramName = ""
                            }
                        })
                    }
                };

                commonCore.Items.Add("Currency:MtxPurchased".ComputeSHA256Hash(), new ProfileItem
                {
                    Attributes = JObject.FromObject(new CurrencyAttributes
                    {
                        Platform = "EpicPC"
                    }),
                    Quantity = commonCoreData.Vbucks,
                    TemplateId = "Currency:MtxPurchased"
                });

                return commonCore;
            }
            catch(Exception ex)
            {
                Logger.Log(ex.ToString());
            }
            return null;
        }

        #endregion

        #region FortniteProfileChanges

        public void SeenAthenaItem(ref AthenaData athenaData, string templateId, bool isSeen)
        {
            var filter = FilterAthenaItem(athenaData.AccountId, templateId);

            var update = Builders<AthenaData>.Update.Set(x => x.Items.FirstMatchingElement().Seen, isSeen);
            athenaData.Items.FirstOrDefault(x => x.ItemIdResponse == templateId).Seen = isSeen;

            _athenaData.UpdateOne(filter, update);
        }

        public void FavoriteAthenaItem(ref AthenaData athenaData, string templateId, bool isFavorite)
        {
            var filter = FilterAthenaItem(athenaData.AccountId, templateId);

            var update = Builders<AthenaData>.Update.Set(x => x.Items.FirstMatchingElement().IsFavorite, isFavorite);
            athenaData.Items.FirstOrDefault(x => x.ItemIdResponse == templateId).IsFavorite = isFavorite;
            _athenaData.UpdateOne(filter, update);
        }

        public void EquipAthenaItem(ref AthenaData athenaData, string itemType, string itemId, int index)
        {
            var filter = Builders<AthenaData>.Filter.Eq(x => x.AccountId, athenaData.AccountId);

            var update = itemType switch
            {
                "Character" => Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentSkin, itemId),
                "Backpack" => Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentBackbling, itemId),
                "Pickaxe" => Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentPickaxe, itemId),
                "SkyDiveContrail" => Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentTrail, itemId),
                "Glider" => Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentGlider, itemId),
                "MusicPack" => Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentMusic, itemId),
                "LoadingScreen" => Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentLoadingScreen, itemId),
                "Dance" => Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentEmotes[index], itemId),
                "ItemWrap" => index is -1 ? ItemWrapSupport(ref athenaData) : Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentWraps[index], itemId),
                _ => throw new BaseException("", $"The item type \"{itemType}\" was not found!", 1142, "")
            };

            _athenaData.UpdateOne(filter, update);

            UpdateDefinition<AthenaData> ItemWrapSupport(ref AthenaData athenaData)
            {
                for (int i = 0; i < 7; i++)
                {
                    var itemWrapUpdate = Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentWraps[i], itemId);

                    _athenaData.UpdateOne(filter, itemWrapUpdate);
                }

                return update = Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentWraps[1], itemId);
            }
        }

        private void UpdateAthena(ref AthenaData athenaData, FilterDefinition<AthenaData> filter, UpdateDefinition<AthenaData> update)
        {
            var filterRvn = Builders<AthenaData>.Update.Set(x => x.Rvn, athenaData.Rvn + 1);

            _athenaData.UpdateOne(filter, filterRvn);
            _athenaData.UpdateOne(filter, update);
        }

        public void UpdateAthenaRvn(ref AthenaData athenaData)
        {
            var filter = Builders<AthenaData>.Filter.Eq(x => x.AccountId, athenaData.AccountId);
            var update = Builders<AthenaData>.Update.Set(x => x.Rvn, athenaData.Rvn + 1);

            _athenaData.UpdateOne(filter, update);
        }

        #endregion
    }
}
