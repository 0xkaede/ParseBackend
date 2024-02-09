using Amazon.Runtime.Internal.Transform;
using CUE4Parse.GameTypes.PUBG.Assets.Exports;
using CUE4Parse.Utils;
using Discord;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParseBackend.Enums;
using ParseBackend.Exceptions;
using ParseBackend.Exceptions.Common;
using ParseBackend.Models.CUE4Parse.Challenges;
using ParseBackend.Models.Database;
using ParseBackend.Models.Database.Athena;
using ParseBackend.Models.Database.CommonCore;
using ParseBackend.Models.Database.Other;
using ParseBackend.Models.Profile;
using ParseBackend.Models.Profile.Attributes;
using ParseBackend.Models.Profile.Stats;
using ParseBackend.Utils;
using Serilog.Context;
using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using static ParseBackend.Global;

namespace ParseBackend.Services
{
    public interface IMongoService
    {
        public void Ping();

        public Task<UserData> LoginAccount(string email, string password);

        public Task<UserData> FindUserByAccountId(string accountId);
        public Task<AthenaData> FindAthenaByAccountId(string accountId);
        public Task<CommonCoreData> FindCommonCoreByAccountId(string accountId);
        public Task<FriendsData> FindFriendsByAccountId(string accountId);
        public Task<MtxAffiliateData> FindSacByCode(string code);

        public Task<UserData> FindUserByAccountName(string un);

        public Task<Profile> CreateAthenaProfile(string accountId);
        public Task<Profile> CreateCommonCoreProfile(string accountId);
        public Task<Profile> CreateCommonPublicProfile(string accountId);

        public void SeenAthenaItem(ref AthenaData athenaData, string templateId, bool isSeen);
        public void EquipAthenaItem(ref AthenaData athenaData, string itemType, string itemId, int index);
        public void FavoriteAthenaItem(ref AthenaData athenaData, string templateId, bool isFavorite);

        public void UpdateAthenaRvn(ref AthenaData athenaData);
        public void UpdateAthenaQuestReRoles(ref AthenaData athenaData, int num);
        public void UpdateAthenaNewDailyQuestsList(ref AthenaData athenaData, Dictionary<string, BaseChallenge> challengeData);
        public void UpdateAthenaQuestLoginTime(ref AthenaData athenaData);
        public void UpdateAthenaItemVariants(ref AthenaData athenaData, string templateId);
        public void AddedAthenaItem(ref AthenaData athenaData, AthenaItemsData athenaItem);
        public void AddedCommonCoreGift(ref CommonCoreData commonData, CommonCoreDataGifts item);
        public void UpdateCommonCoreGift(string accountId, List<CommonCoreDataGifts> data);
        public void UpdateCommonCoreVbucks(ref CommonCoreData commonCoreData);
        public void UpdateCommonCoreRvn(ref CommonCoreData commonCoreData);
        public void UpdateSac(ref CommonCoreData commonCoreData, string code);
        public void UpdateAthenaQuestAssist(ref AthenaData athenaData, string quest);

        public Task<string> FindExchangeCode(string code);

        public void UpdateFriendsStatus(string accountId, string friendId, FriendsStatus status);
        public void AddItemToFriends(string accountId, FriendsListData data);
        public void UpdateFriendsInList(string accountId, string friendId, FriendsListData data);
        public void UpdateFriendsList(string accountId, List<FriendsListData> data);
    }

    public partial class MongoService : IMongoService
    {
        private readonly IFileProviderService _fileProviderService;
        private readonly IMongoCollection<UserData> _userProfiles;
        private readonly IMongoCollection<AthenaData> _athenaData;
        private readonly IMongoCollection<CommonCoreData> _commonCoreData;
        private readonly IMongoCollection<ExchangeCode> _exchangeCodes;
        private readonly IMongoCollection<FriendsData> _friendsData;
        private readonly IMongoCollection<MtxAffiliateData> _mxtAffiliateData;

        public MongoService(IFileProviderService fileProviderService)
        {
            _fileProviderService = fileProviderService;

            var client = new MongoClient("mongodb://localhost");

            var mongoDatabase = client.GetDatabase("ParseBackend");

            _userProfiles = mongoDatabase.GetCollection<UserData>("user_data");
            _athenaData = mongoDatabase.GetCollection<AthenaData>("athena_data");
            _commonCoreData = mongoDatabase.GetCollection<CommonCoreData>("common_core_data");
            _exchangeCodes = mongoDatabase.GetCollection<ExchangeCode>("exchange_codes");
            _friendsData = mongoDatabase.GetCollection<FriendsData>("friends_data");
            _mxtAffiliateData = mongoDatabase.GetCollection<MtxAffiliateData>("sac_data");

            _ = InitDatabase();
        }

        public string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void Ping()
        {
            Logger.Log("Mongodb is online");
        }

        public async Task InitDatabase()
        {
            /*Logger.Log("Database Is Online");
            for(int i = 0; i < 3000; i++)
            {
                var test = RandomString(12);
                await CreateAccount(test, test, test);
            }*/
            //GrantAthenaFullLocker("78bf7fe5e26d454f902cf55c5d54f775");
            /*var sw = new Stopwatch();
            sw.Start();
            await GrantAthenaFullLockerAsync("65a503ba4c7943e5a8d5281133415ed5");
            sw.Stop();
            Logger.Log($"Time taken {sw.Elapsed.TotalSeconds}");*/

            _mxtAffiliateData.InsertOne(new MtxAffiliateData
            {
                AccountId = "b24912453f58465991dbd51ea1a2d6b4",
                Code = "kaede",
                Purchaces = 0,
                VbuckSpent = 0,
            });
            await GrantAthenaFullLockerAsync("d7b0380390094695857e9da7ec29306f");
            await CreateAccount("kaede@fn.dev", "kaede1234", "Kaede");
            await CreateAccount("kaede@fort.dev", "kaede1234", "Kaede2");
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
            var user = await _userProfiles.FindAsync(x => x.AccountId == accountId);
            return user.First();
        }

        public async Task<UserData> FindUserByAccountName(string un)
        {
            var user = await _userProfiles.FindAsync(x => x.Username == un);
            return user.First();
        }

        public async Task<AthenaData> FindAthenaByAccountId(string accountId)
        {
            var users = await _athenaData.FindAsync(x => x.AccountId == accountId);
            return users.First();
        }

        public async Task<CommonCoreData> FindCommonCoreByAccountId(string accountId)
        {
            var users = await _commonCoreData.FindAsync(x => x.AccountId == accountId);
            return users.First();
        }

        public async Task<FriendsData> FindFriendsByAccountId(string accountId)
        {
            var user = await _friendsData.FindAsync(x => x.AccountId == accountId);
            return user.First();
        }

        public async Task<MtxAffiliateData> FindSacByCode(string code)
        {
            var user = await _mxtAffiliateData.FindAsync(x => x.Code == code);
            return user.First();
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
                        CurrentPickaxe = "AthenaPickaxe:DefaultPickaxe".ComputeSHA256Hash(),
                        CurrentGlider = "AthenaGlider:DefaultGlider".ComputeSHA256Hash(),
                        CurrentEmotes = new List<string>() { "", "", "", "", "", "" },
                        CurrentWraps = new List<string>() { "", "", "", "", "", "", "" },
                        CurrentLoadingScreen = "",
                        CurrentMusic = "",
                        CurrentTrail = "",
                    },
                    BattleBoost = 0,
                    BattleBoostFriend = 0,
                    QuestAssist = ""
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
                                CurrentPickaxe = "AthenaPickaxe:DefaultPickaxe".ComputeSHA256Hash(),
                                CurrentGlider = "AthenaGlider:DefaultGlider".ComputeSHA256Hash(),
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
                Gifts = new List<CommonCoreDataGifts>(),
                Items = new List<CommonCoreItems>(),
                Rvn = 0,
                Vbucks = 0,
                Stats = new CommonCoreDataStats
                {
                    MtxAffiliate = "",
                    MtxAffiliateTime = DateTime.Now,
                    GiftRemaining = 5,
                    LastGiftRefresh = DateTime.Now,
                    ReciveGifts = true
                },
            };

            await _athenaData.InsertOneAsync(athenaData);
            await _commonCoreData.InsertOneAsync(commonCoreData);
            await _userProfiles.InsertOneAsync(userData);
            await _friendsData.InsertOneAsync(new FriendsData
            {
                AccountId = id,
                List = new List<FriendsListData>(),
            });

            Logger.Log($"Account Created: {username}");
        }

        public async Task<UserData> LoginAccount(string email, string password)
        {
            var users = await GetAllUserProfiles();

            var emailCheck = users.FirstOrDefault(x => x.Email == email);
            if (email is null)
                throw new BaseException("", "Email wasnt found", 1008, "");

            if (emailCheck.Password != password.ComputeSHA256Hash())
                throw new BaseException("", "Password is wrong, Please try again!", 1008, "");

            return emailCheck;
        }

        public async Task GrantAthenaFullLockerAsync(string accountId)
        {
            var itemsToGrant = new List<AthenaItemsData>();
            var itemsFromFile = _fileProviderService.GetAllCosmetics();

            foreach (var item in itemsFromFile)
            {
                try
                {
                    var itemToLower = item.ToLower();
                    var itemRaw = item.SubstringAfterLast("/").SubstringBefore(".");

                    var itemFixed = FixCosmetic(itemRaw, itemToLower);
                    if (itemFixed == null) continue;

                    var variant = await _fileProviderService.GetCosmeticsVariants(item.SubstringBefore("."));

                    itemsToGrant.Add(new AthenaItemsData
                    {
                        Amount = 1,
                        Seen = false,
                        IsFavorite = false,
                        ItemId = itemFixed,
                        ItemIdResponse = itemFixed.ComputeSHA256Hash(),
                        Variants = variant
                    });
                }
                catch
                {
                    continue;
                }
                
            }

            var filter = Builders<AthenaData>.Filter.Eq(x => x.AccountId, accountId);
            var update = Builders<AthenaData>.Update.Set(x => x.Items, itemsToGrant);

            _athenaData.UpdateOne(filter, update);

            string FixCosmetic(string itemRaw, string toLowerItem)
            {
                if (toLowerItem.Contains("random")) return null;

                if (toLowerItem.Contains("characters"))
                    return $"AthenaCharacter:{itemRaw}";

                if (toLowerItem.Contains("backpacks") || toLowerItem.Contains("pets"))
                    return $"AthenaBackpack:{itemRaw}";

                if (toLowerItem.Contains("pickaxe"))
                    return $"AthenaPickaxe:{itemRaw}";

                if (toLowerItem.Contains("dance") || toLowerItem.Contains("sprays") || toLowerItem.Contains("toys"))
                    return $"AthenaDance:{itemRaw}";

                if (toLowerItem.Contains("musicpacks"))
                    return $"AthenaMusicPack:{itemRaw}";

                if (toLowerItem.Contains("loadingscreens"))
                    return $"AthenaLoadingScreen:{itemRaw}";

                if (toLowerItem.Contains("wraps"))
                    return $"AthenaItemWrap:{itemRaw}";

                if (toLowerItem.Contains("gliders"))
                    return $"AthenaGlider:{itemRaw}";

                if (toLowerItem.Contains("contrail"))
                    return $"AthenaSkyDiveContrail:{itemRaw}";

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
                        PartyAssistQuest = athenaData.Stats.QuestAssist
                    })
                }
            };

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
                                {
                                    
                                }
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
                            },
                            MtxAffiliateId = commonCoreData.Stats.MtxAffiliate,
                            MtxAffiliateSetTime = commonCoreData.Stats.MtxAffiliateTime
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

                if(commonCoreData.Gifts.Count != 0)
                {
                    var gifts = commonCoreData.Gifts;
                    foreach(var gift in gifts)
                    {
                        var loot = new List<GiftBoxLootList>();

                        foreach(var item in gift.LootList)
                        {
                            var variant = await _fileProviderService.GetCosmeticsVariants(item.Contains(":") ? item.Split(":")[1] : item);

                            loot.Add(new GiftBoxLootList
                            {
                                ItemType = item,
                                ItemGuid = item.ComputeSHA256Hash(),
                                ItemProfile = "athena",
                                Quantity = 1
                            });
                        }


                        commonCore.Items.Add(gift.TemplateIdHashed, new ProfileItem
                        {
                            TemplateId = gift.TemplateId,
                            Attributes = new GiftBoxAttribute
                            {
                                FromAccountId = gift.FromAccountId,
                                GiftedOn = gift.Time.TimeToString(),
                                Level = 1,
                                LootList = loot,
                                Params = new Dictionary<string, string>
                                {
                                    {
                                        "userMessage",
                                        gift.UserMessage
                                    }
                                }
                            },
                            Quantity = 1
                        });
                    }
                }


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
                "ItemWrap" => index is -1 ? ItemWrapSupport() : Builders<AthenaData>.Update.Set(x => x.Stats.CurrentItems.CurrentWraps[index], itemId),
                _ => throw new BaseException("", $"The item type \"{itemType}\" was not found!", 1142, "")
            };

            _athenaData.UpdateOne(filter, update);

            UpdateDefinition<AthenaData> ItemWrapSupport()
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

            athenaData.Rvn += 1;

            _athenaData.UpdateOne(filter, update);
        }

        public void UpdateAthenaQuestReRoles(ref AthenaData athenaData, int num)
        {
            var filter = Builders<AthenaData>.Filter.Eq(x => x.AccountId, athenaData.AccountId);
            var update = Builders<AthenaData>.Update.Set(x => x.DailyQuestData.DailyQuestRerolls, num);

            athenaData.DailyQuestData.DailyQuestRerolls = num;

            _athenaData.UpdateOne(filter, update);
        }

        public void UpdateAthenaQuestLoginTime(ref AthenaData athenaData)
        {
            var filter = Builders<AthenaData>.Filter.Eq(x => x.AccountId, athenaData.AccountId);
            var update = Builders<AthenaData>.Update.Set(x => x.DailyQuestData.DailyLoginInterval, DateTime.Now);
            var update2 = Builders<AthenaData>.Update.Set(x => x.DailyQuestData.DailyLoginIntervalString, CurrentTime());

            athenaData.DailyQuestData.DailyLoginInterval = DateTime.Now;

            _athenaData.UpdateOne(filter, update);
            _athenaData.UpdateOne(filter, update2);
        }

        public void UpdateAthenaNewDailyQuestsList(ref AthenaData athenaData, Dictionary<string, BaseChallenge> challengeData)
        {
            var updateList = new List<AthenaChallengeData>();

            foreach(var challenge in challengeData)
            {
                var data = new AthenaChallengeData
                {
                    ParentAsset = "",
                    Objectives = new List<string>(),
                    ItemId = challenge.Key
                };

                foreach(var chal in challenge.Value.Objects)
                    data.Objectives.Add($"completion_{chal.Key}:0");

                updateList.Add(data);
            }

            var filter = Builders<AthenaData>.Filter.Eq(x => x.AccountId, athenaData.AccountId);
            var update = Builders<AthenaData>.Update.Set(x => x.DailyQuestData.Quests, updateList);

            athenaData.DailyQuestData.Quests = updateList;

            _athenaData.UpdateOne(filter, update);
        }

        public void UpdateAthenaItemVariants(ref AthenaData athenaData, string templateId)
        {
            var filter = FilterAthenaItem(athenaData.AccountId, templateId);

            var variant = athenaData.Items.FirstOrDefault(x => x.ItemIdResponse == templateId)!.Variants;

            var update = Builders<AthenaData>.Update.Set(x => x.Items.FirstMatchingElement().Variants, variant);

            _athenaData.UpdateOne(filter, update);
        }

        public void AddedAthenaItem(ref AthenaData athenaData, AthenaItemsData athenaItem)
        {
            var filter = Builders<AthenaData>.Filter.Eq(x => x.AccountId, athenaData.AccountId);

            var update = Builders<AthenaData>.Update.Push(x => x.Items, athenaItem);

            _athenaData.UpdateOne(filter, update);
        }

        public void AddedCommonCoreGift(ref CommonCoreData commonData, CommonCoreDataGifts item)
        {
            var filter = Builders<CommonCoreData>.Filter.Eq(x => x.AccountId, commonData.AccountId);

            var update = Builders<CommonCoreData>.Update.Push(x => x.Gifts, item);

            _commonCoreData.UpdateOne(filter, update);
        }

        public void UpdateCommonCoreGift(string accountId, List<CommonCoreDataGifts> data) //skunky ik
        {
            var filter = Builders<CommonCoreData>.Filter.Eq(x => x.AccountId, accountId);

            var update = Builders<CommonCoreData>.Update.Set(x => x.Gifts, data);

            _commonCoreData.UpdateOne(filter, update);
        }

        public void UpdateCommonCoreVbucks(ref CommonCoreData commonCoreData)
        {
            var filter = Builders<CommonCoreData>.Filter.Eq(x => x.AccountId, commonCoreData.AccountId);
            var update = Builders<CommonCoreData>.Update.Set(x => x.Vbucks, commonCoreData.Vbucks);

            _commonCoreData.UpdateOne(filter, update);
        }

        public void UpdateCommonCoreRvn(ref CommonCoreData commonCoreData)
        {
            var filter = Builders<CommonCoreData>.Filter.Eq(x => x.AccountId, commonCoreData.AccountId);
            var update = Builders<CommonCoreData>.Update.Set(x => x.Rvn, commonCoreData.Rvn + 1);

            commonCoreData.Rvn += 1;

            _commonCoreData.UpdateOne(filter, update);
        }

        public void UpdateSac(ref CommonCoreData commonCoreData, string code)
        {
            var filter = Builders<CommonCoreData>.Filter.Eq(x => x.AccountId, commonCoreData.AccountId);
            var update = Builders<CommonCoreData>.Update.Set(x => x.Stats.MtxAffiliate, code);
            var linkedTime = DateTime.Now;
            var update2 = Builders<CommonCoreData>.Update.Set(x => x.Stats.MtxAffiliateTime, linkedTime);

            commonCoreData.Stats.MtxAffiliate = code;
            commonCoreData.Stats.MtxAffiliateTime = linkedTime;

            _commonCoreData.UpdateOne(filter, update);
            _commonCoreData.UpdateOne(filter, update2);
        }

        public void UpdateAthenaQuestAssist(ref AthenaData athenaData, string quest)
        {
            var filter = Builders<AthenaData>.Filter.Eq(x => x.AccountId, athenaData.AccountId);
            var update = Builders<AthenaData>.Update.Set(x => x.Stats.QuestAssist, quest);

            athenaData.Stats.QuestAssist = quest;

            _athenaData.UpdateOne(filter, update);
        }

        #endregion

        #region ExchangeCode

        public async Task<string> CreateExchangeCode(string accountId)
        {
            var code = RandomString(6);

            await _exchangeCodes.InsertOneAsync(new ExchangeCode
            {
                AccountId = accountId,
                Code = code,
                DateCreated = DateTime.Now,
            });

            return code;
        }

        public async Task<string> FindExchangeCode(string code)
        {
            var exchangeFind = await _exchangeCodes.FindAsync(x => x.Code == code);
            var exchangeData = exchangeFind.First();

            if (exchangeData.DateCreated.AddMinutes(5) < DateTime.Now)
                throw new BaseException("", "Sorry the exchange code you supplied was not found. It is possible that it was no longer valid", 1315, "");

            return exchangeData.AccountId;
        }

        #endregion

        #region Friends
        public async void AddItemToFriends(string accountId, FriendsListData data)
        {
            var filter = Builders<FriendsData>.Filter.Eq(x => x.AccountId, accountId);

            var update = Builders<FriendsData>.Update.Push<FriendsListData>(x => x.List, data);

            _friendsData.UpdateOne(filter, update);
        }

        public void UpdateFriendsStatus(string accountId, string friendId, FriendsStatus status)
        {
            var filter = Builders<FriendsData>.Filter.Eq(x => x.AccountId, accountId)
            & Builders<FriendsData>.Filter.ElemMatch(x => x.List, Builders<FriendsListData>.Filter.Eq(x => x.AccountId, friendId));

            var update = Builders<FriendsData>.Update.Set(x => x.List.FirstMatchingElement().Status, status);

            _friendsData.UpdateOne(filter, update);
        }

        public void UpdateFriendsInList(string accountId, string friendId, FriendsListData data)
        {
            var filter = Builders<FriendsData>.Filter.Eq(x => x.AccountId, accountId)
            & Builders<FriendsData>.Filter.ElemMatch(x => x.List, Builders<FriendsListData>.Filter.Eq(x => x.AccountId, friendId));

            var update = Builders<FriendsData>.Update.Set(x => x.List.FirstMatchingElement(), data);

            _friendsData.UpdateOne(filter, update);
        }

        public void UpdateFriendsList(string accountId, List<FriendsListData> data)
        {
            var filter = Builders<FriendsData>.Filter.Eq(x => x.AccountId, accountId);

            var update = Builders<FriendsData>.Update.Set(x => x.List, data);

            _friendsData.UpdateOne(filter, update);
        }
        #endregion

        public void UpdateSacVbucksByCode(string code, int amount)
        {
            var filter = Builders<MtxAffiliateData>.Filter.Eq(x => x.Code, code);

            var update = Builders<MtxAffiliateData>.Update.Set(x => x.VbuckSpent, amount);

            _mxtAffiliateData.UpdateOne(filter, update);
        }
    }
}
