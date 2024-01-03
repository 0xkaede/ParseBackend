using Amazon.Runtime.Internal.Transform;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ParseBackend.Exceptions;
using ParseBackend.Exceptions.Common;
using ParseBackend.Models.Database;
using ParseBackend.Models.Profiles;
using ParseBackend.Utils;
using static ParseBackend.Global;

namespace ParseBackend.Services
{
    public interface IMongoService
    {
        public Task<UserData> LoginAccount(string email, string password);

        public Task<UserData> FindUserByAccountId(string accountId);
    }

    public class MongoService : IMongoService
    {
        private readonly IMongoCollection<UserData> _userProfiles;
        private readonly IMongoCollection<AthenaData> _athenaData;

        public MongoService()
        {
            var client = new MongoClient("mongodb://localhost");

            var mongoDatabase = client.GetDatabase("KaedeBackend");

            _userProfiles = mongoDatabase.GetCollection<UserData>("UserProfiles");
            _athenaData = mongoDatabase.GetCollection<AthenaData>("AthenaData");

            _ = InitDatabase();
        }

        public async Task InitDatabase()
        {
            Logger.Log("Database Is Online");
            await CreateAccount("kaede@fn.dev", "kaede1234", "Kaede");
        }

        public async Task<List<UserData>> GetAllUserProfiles()
        {
            var item = await _userProfiles.FindAsync(x => true);
            return item.ToList();
        }

        public async Task<List<AthenaData>> GetAllUserAthenaProfiles()
        {
            var item = await _athenaData.FindAsync(x => true);
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
                    Hours = 0,
                    Reason = null
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
                athenaData.Items.Add(new AthenaItemsData
                {
                    Amount = 1,
                    Seen = false,
                    IsFavorite = false,
                    ItemId = item
                });
            }

            await _athenaData.InsertOneAsync(athenaData);
            await _userProfiles.InsertOneAsync(userData);

            Logger.Log($"Account Created: {username}");
        }

        /*public void LoginAccount(string email, string password, ref UserData data)
        {
            var users = GetAllUserProfiles().Result;

            var emailCheck = users.FirstOrDefault(x => x.Email.Equals(email));
            if (email is null)
                throw new BaseException("", "Email wasnt found", 1008, "");

            if (emailCheck.Password != password.ComputeSHA256Hash())
                throw new BaseException("", "Password is wrong, Please try again!", 1008, "");

            data = emailCheck;
        }*/

        public async Task<UserData> LoginAccount(string email, string password)
        {
            var users = await GetAllUserProfiles();

            var emailCheck = users.FirstOrDefault(x => x.Email.Equals(email));
            if (email is null)
                throw new BaseException("", "Email wasnt found", 1008, "");

            if (emailCheck.Password != password.ComputeSHA256Hash())
                throw new BaseException("", "Password is wrong, Please try again!", 1008, "");

            return emailCheck;
        }

        public async Task<AthenaProfile> CreateAthenaProfile(string accountId)
        {
            var user = await FindUserByAccountId(accountId);
            var athenaData = await FindAthenaByAccountId(accountId);

            var athena = new AthenaProfile
            {
                Created = user.Created,
                AccountId = accountId,
                ProfileId = "athena",
                Revision = athenaData.Rvn, //todo
                WipeNumber = 0,
                CommandRevision = athenaData.Rvn,
                Updated = CurrentTime(),
                Version = "no_version",
                Stats = new AthenaStats
                {
                    Attributes = new AthenaAtrributes
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
                    }
                }
            };

            foreach(var item in athenaData.Items)
            {
                athena.Items.Add(item.ItemId.ComputeSHA256Hash(), new ProfileItem
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
        
    }
}
