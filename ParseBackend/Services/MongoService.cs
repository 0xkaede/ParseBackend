using CUE4Parse.Utils;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using ParseBackend.Enums;
using ParseBackend.Enums.Other;
using ParseBackend.Exceptions;
using ParseBackend.Exceptions.AccountService;
using ParseBackend.Models.FortniteService.Profile;
using ParseBackend.Models.FortniteService.Profile.Attributes;
using ParseBackend.Models.FortniteService.Storefront;
using ParseBackend.Models.Other.Cache;
using ParseBackend.Models.Other.Database;
using ParseBackend.Models.Other.Database.Athena;
using ParseBackend.Models.Other.Database.CommonCore;
using ParseBackend.Models.Other.Database.Other;
using ParseBackend.Utils;
using static ParseBackend.Global;

namespace ParseBackend.Services
{
    public interface IMongoService
    {
        public void Ping();

        public Task<UserData> ReadUserData(string pattern, DatabaseSearchType type = DatabaseSearchType.AccountId);
        public Task<FriendsData> ReadFriendsData(string accountId);
        public Task<ProfileCache> GetAllProfileData(string accountId);
        public void SaveAllProfileData(string accountId, ProfileCache profileCache);

        public Task<string> GetAccountIdFromDiscordId(string discordId);
        public Task GrantAthenaFullLockerAsync(string accountId);

        public Task<UserData> LoginAccount(string email, string password);
        public Task<CreateAccountResponse> CreateAccount(string email, string password, string username, string discordId);

        public Task<MtxAffiliateData> FindSacByCode(string code);

        public Task<Profile> CreateCommonPublicProfile(string accountId);

        public Task<string> CreateExchangeCode(string accountId);
        public Task<string> FindExchangeCode(string code);
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
            
        }

        public async Task<UserData> ReadUserData(string pattern, DatabaseSearchType type = DatabaseSearchType.AccountId)
        {
            if(type is DatabaseSearchType.AccountId)
                if (GlobalCacheProfiles.TryGetValue(pattern, out var profile))
                    return profile.UserData;

            if (type is DatabaseSearchType.Username)
            {
                var data = GlobalCacheProfiles.FirstOrDefault(x => x.Value.UserData.Username == pattern).Value;
                if(data != null)
                    return data.UserData;
            }

            var users = type switch
            {
                DatabaseSearchType.AccountId => await _userProfiles.FindAsync(x => x.AccountId == pattern),
                DatabaseSearchType.Username => await _userProfiles.FindAsync(x => x.Username == pattern)
            };

            return users.First();
        }

        public async Task<FriendsData> ReadFriendsData(string accountId) //used for readonly things
        {
            if (GlobalCacheProfiles.TryGetValue(accountId, out var profile))
                return profile.FriendsData;

            var profiles = await _friendsData.FindAsync(x => x.AccountId == accountId);
            return profiles.First();
        }

        public async Task<ProfileCache> GetAllProfileData(string accountId)
        {
            try
            {
                if (GlobalCacheProfiles.TryGetValue(accountId, out var profile))
                {
                    profile.LastChanges = DateTime.Now; //stop caching fucking its self
                    return profile;
                }

                var user = await _userProfiles.FindAsync(x => x.AccountId == accountId);
                var athena = await _athenaData.FindAsync(x => x.AccountId == accountId);
                var common = await _commonCoreData.FindAsync(x => x.AccountId == accountId);
                var friend = await _friendsData.FindAsync(x => x.AccountId == accountId);

                var cache = new ProfileCache
                {
                    UserData = user.First(),
                    AthenaData = athena.First(),
                    CommonData = common.First(),
                    FriendsData = friend.First(),
                    LastChanges = DateTime.Now,
                };

                if (GlobalCacheProfiles.FirstOrDefault(x => x.Key == accountId).Value is null)
                    GlobalCacheProfiles.Add(accountId, cache);

                return GlobalCacheProfiles[accountId];
            }
            catch (ArgumentException ex)
            {
                if(ex.ToString().Contains("An item with the same key has already been added."))
                    return GlobalCacheProfiles[accountId];
            }

            return GlobalCacheProfiles[accountId];
        }

        public void SaveAllProfileData(string accountId, ProfileCache profileCache)
        {
            var filter1 = Builders<UserData>.Filter.Eq(x => x.AccountId, accountId);

            var filter2 = Builders<AthenaData>.Filter.Eq(x => x.AccountId, accountId);
            var update2 = Builders<AthenaData>.Update.Set(x => x, profileCache.AthenaData);

            var filter3 = Builders<CommonCoreData>.Filter.Eq(x => x.AccountId, accountId);
            var update3 = Builders<CommonCoreData>.Update.Set(x => x, profileCache.CommonData);

            var filter4 = Builders<FriendsData>.Filter.Eq(x => x.AccountId, accountId);
            var update4 = Builders<FriendsData>.Update.Set(x => x, profileCache.FriendsData);

            _userProfiles.ReplaceOne(filter1, profileCache.UserData);
            _athenaData.ReplaceOne(filter2, profileCache.AthenaData);
            _commonCoreData.ReplaceOne(filter3, profileCache.CommonData);
            _friendsData.ReplaceOne(filter4, profileCache.FriendsData);

            Logger.Log("saved user data");
        }

        public async Task<string> GetAccountIdFromDiscordId(string discordId)
        {
            var users = await _userProfiles.FindAsync(x => x.DiscordId == discordId);
            return users.First().AccountId;
        }

        public async Task<MtxAffiliateData> FindSacByCode(string code)
        {
            var user = await _mxtAffiliateData.FindAsync(x => x.Code == code);
            return user.First();
        }

        public async Task<CreateAccountResponse> CreateAccount(string email, string password, string username, string discordId)
        {
            var usersTemp = await _userProfiles.FindAsync(x => true);
            var users = usersTemp.ToList();

            var discordCheck = users.FirstOrDefault(x => x.DiscordId == discordId);
            if (discordCheck != null)
                return CreateAccountResponse.DiscordId;

            var usernameCheck = users.FirstOrDefault(x => x.Username == username);
            if (usernameCheck != null)
                return CreateAccountResponse.Username;

            var emailChekc = users.FirstOrDefault(x => x.Email == email);
            if (emailChekc != null)
                return CreateAccountResponse.Email;

            var id = CreateUuid();
            var time = CurrentTime();

            var userData = new UserData
            {
                AccountId = id,
                DiscordId = discordId,
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
                Rvn = 1,
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
                Rvn = 1,
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

            return CreateAccountResponse.Created;
        }

        public async Task<UserData> LoginAccount(string email, string password)
        {
            var check = await _userProfiles.FindAsync(x => x.Email == email);
            var user = check.First();

            if (user is null)
                throw new BaseException("", "Email wasnt found", 1008, "");

            if (user.Password != password.ComputeSHA256Hash())
                throw new BaseException("", "Password is wrong, Please try again!", 1008, "");

            return user;
        }

        public async Task GrantAthenaFullLockerAsync(string accountId)
        {
            var giftBoxItemId = CreateUuid();

            var itemsToGrant = new List<AthenaItemsData>();
            var itemsFromFile = _fileProviderService.GetAllCosmetics();
            itemsFromFile = itemsFromFile.OrderBy(x => x).ToList();

            var gb = new CommonCoreDataGifts
            {
                FromAccountId = CreateUuid(),
                LootList = new List<string>(),
                TemplateId = "GiftBox:GB_MakeGood",
                TemplateIdHashed = giftBoxItemId,
                Time = DateTime.Now,
                UserMessage = "You have been granted full locker!"
            };

            foreach (var item in itemsFromFile)
            {
                try
                {
                    var itemToLower = item.ToLower();
                    var itemRaw = item.SubstringAfterLast("/").SubstringBefore(".");

                    var itemFixed = FixCosmetic(itemRaw, itemToLower);
                    if (itemFixed == null) continue;

                    var variant = await _fileProviderService.GetCosmeticsVariants(item.SubstringBefore("."));

                    gb.LootList.Add(itemFixed);

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

            var profiles = await GetAllProfileData(accountId);

            profiles.AthenaData.Items = itemsToGrant;

            profiles.CommonData.Gifts.Add(gb);

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

        public void UpdateSacVbucksByCode(string code, int amount)
        {
            var filter = Builders<MtxAffiliateData>.Filter.Eq(x => x.Code, code);

            var update = Builders<MtxAffiliateData>.Update.Set(x => x.VbuckSpent, amount);

            _mxtAffiliateData.UpdateOne(filter, update);
        }
    }
}
