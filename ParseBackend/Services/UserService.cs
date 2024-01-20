using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ParseBackend.Exceptions;
using ParseBackend.Models.CUE4Parse.Challenges;
using ParseBackend.Models.Database.Athena;
using ParseBackend.Models.Profile;
using ParseBackend.Models.Profile.Attributes;
using ParseBackend.Models.Profile.Changes;
using ParseBackend.Models.Request;
using ParseBackend.Models.Response;
using ParseBackend.Utils;
using System;
using System.Security.Authentication;
using static ParseBackend.Global;

namespace ParseBackend.Services
{
    public interface IUserService
    {
        public Task<ProfileResponse> QueryProfile(string type, string accountId);
        public Task<ProfileResponse> EquipBattleRoyaleCustomization(string accountId, EquipBattleRoyaleCustomizationRequest body);
        public Task<ProfileResponse> MarkItemSeen(string accountId, MarkItemSeenRequest body);
        public Task<ProfileResponse> SetItemFavoriteStatusBatch(string accountId, SetItemFavoriteStatusBatchRequest body);
        public Task<ProfileResponse> ClientQuestLogin(string accountId);
        public Task<ProfileResponse> SetPartyAssistQuest(string accountId, JObject lazy);
    }

    public class UserService : IUserService
    {
        private readonly IMongoService _mongoService;
        private readonly IFileProviderService _fileProviderService;

        public UserService(IMongoService mongoService, IFileProviderService fileProviderService)
        {
            _mongoService = mongoService;
            _fileProviderService = fileProviderService;
        }

        public ProfileResponse CreateProfileResponse(ref Profile profile, List<object> profileChanges = null)
            => new ProfileResponse
            {
                ProfileRevision = profile.Revision + 1,
                ProfileId = profile.ProfileId,
                ProfileChangesBaseRevisionRevision = profile.Revision,
                ProfileChanges = profileChanges ?? new List<object>(),
                ProfileCommandRevision = profile.Revision + 1,
                ServerTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                ResponseVersion = 1
            };

        public ProfileResponse CreateProfileResponse(ref AthenaData profile, List<object> profileChanges = null)
            => new ProfileResponse
            {
                ProfileRevision = profile.Rvn + 1,
                ProfileId = "athena",
                ProfileChangesBaseRevisionRevision = profile.Rvn,
                ProfileChanges = profileChanges ?? new List<object>(),
                ProfileCommandRevision = profile.Rvn + 1,
                ServerTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                ResponseVersion = 1
            };

        public async Task<ProfileResponse> QueryProfile(string type, string accountId)
        {
            Logger.Log($"{type} : {accountId}");
            var profile = type switch
            {
                "athena" => await _mongoService.CreateAthenaProfile(accountId),
                "common_core" => await _mongoService.CreateCommonCoreProfile(accountId),
                "common_public" => await _mongoService.CreateCommonPublicProfile(accountId),
            };

            var profileChanges = new List<object>();

            profileChanges.Add(new FullProfileUpdate // what was i doing
            {
                Profile = profile
            });

            return CreateProfileResponse(ref profile, profileChanges);
        }

        public async Task<ProfileResponse> EquipBattleRoyaleCustomization(string accountId, EquipBattleRoyaleCustomizationRequest body)
        {
            var athenaData = await _mongoService.FindAthenaByAccountId(accountId);

            _mongoService.EquipAthenaItem(ref athenaData, body.SlotName, body.ItemToSlot, body.IndexWithinSlot);

            var profileChanges = new List<object>();

            var poop = body.SlotName.ToLower().Contains("wrap") ? "itemwraps" : body.SlotName.ToLower();

            var data = new StatModified
            {
                Name = $"favorite_{poop}",
                Value = body.ItemToSlot.ToString()
            };

            athenaData = await _mongoService.FindAthenaByAccountId(accountId); //idk how bad this is

            data.Value = body.SlotName switch
            {
                "Character" => athenaData.Stats.CurrentItems.CurrentSkin,
                "Backpack" => athenaData.Stats.CurrentItems.CurrentBackbling,
                "Pickaxe" => athenaData.Stats.CurrentItems.CurrentPickaxe,
                "SkyDiveContrail" => athenaData.Stats.CurrentItems.CurrentTrail,
                "Glider" => athenaData.Stats.CurrentItems.CurrentGlider,
                "MusicPack" => athenaData.Stats.CurrentItems.CurrentMusic,
                "LoadingScreen" => athenaData.Stats.CurrentItems.CurrentLoadingScreen,
                "Dance" => ItemArrays(),
                "ItemWrap" => ItemArrays(),
                _ => throw new BaseException("", $"The item type \"{body.SlotName}\" was not found!", 1142, "")
            };

            object ItemArrays()
            {
                if(body.SlotName is "Dance")
                {
                    athenaData.Stats.CurrentItems.CurrentEmotes[body.IndexWithinSlot] = body.ItemToSlot;
                    return athenaData.Stats.CurrentItems.CurrentEmotes;
                }

                if(body.SlotName is "ItemWrap")
                {
                    if(body.IndexWithinSlot is -1)
                        for (int i = 0; i < 7; i++)
                            athenaData.Stats.CurrentItems.CurrentWraps[i] = body.ItemToSlot;
                    else
                        athenaData.Stats.CurrentItems.CurrentWraps[body.IndexWithinSlot] = body.ItemToSlot;

                    return athenaData.Stats.CurrentItems.CurrentWraps;
                }

                return null;
            }

            profileChanges.Add(data);

            _mongoService.UpdateAthenaRvn(ref athenaData);

            return CreateProfileResponse(ref athenaData, profileChanges);
        }

        public async Task<ProfileResponse> MarkItemSeen(string accountId, MarkItemSeenRequest body)
        {
            var athenaData = await _mongoService.FindAthenaByAccountId(accountId);

            var profileChanges = new List<object>();

            foreach (var item in body.ItemIds)
            {
                _mongoService.SeenAthenaItem(ref athenaData, item, true);

                profileChanges.Add(new ItemAttrChanged
                {
                    ItemId = item,
                    AttributeName = "item_seen",
                    AttributeValue = true
                });
            }

            _mongoService.UpdateAthenaRvn(ref athenaData);

            return CreateProfileResponse(ref athenaData, profileChanges);
        }

        public async Task<ProfileResponse> SetItemFavoriteStatusBatch(string accountId, SetItemFavoriteStatusBatchRequest body)
        {
            var athenaData = await _mongoService.FindAthenaByAccountId(accountId);

            var profileChanges = new List<object>();

            for (var i = 0; i < body.ItemIds.Count(); i++)
            {
                _mongoService.FavoriteAthenaItem(ref athenaData, body.ItemIds[i], body.ItemFavStatus[i]);

                profileChanges.Add(new ItemAttrChanged
                {
                    ItemId = body.ItemIds[i],
                    AttributeName = "favorite",
                    AttributeValue = body.ItemFavStatus[i]
                });
            }

            _mongoService.UpdateAthenaRvn(ref athenaData);

            return CreateProfileResponse(ref athenaData, profileChanges);
        }

        public async Task<ProfileResponse> ClientQuestLogin(string accountId) // not finished
        {
            var athenaData = await _mongoService.FindAthenaByAccountId(accountId);

            var profileChanges = new List<object>();

            if (athenaData.DailyQuestData.DailyLoginInterval.AddHours(24) < DateTime.Now) //new challenges
            {
                var questList = await _fileProviderService.GenerateDailyQuest();

                if(athenaData.DailyQuestData.DailyQuestRerolls <= 0)
                    _mongoService.UpdateAthenaQuestReRoles(ref athenaData, 1);

                _mongoService.UpdateAthenaQuestLoginTime(ref athenaData);

                _mongoService.UpdateAthenaNewDailyQuestsList(ref athenaData, questList);
            }

            foreach (var quest in athenaData.DailyQuestData.Quests)
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

                profileChanges.Add(new ItemAdded
                {
                    Item = new ProfileItem
                    {
                        Attributes = data,
                        Quantity = 1,
                        TemplateId = $"Quest:{quest.ItemId}"
                    },
                    ItemId = $"Quest:{quest.ItemId}".ComputeSHA256Hash()
                });
            }

            _mongoService.UpdateAthenaRvn(ref athenaData);

            return CreateProfileResponse(ref athenaData, profileChanges);
        }

        public async Task<ProfileResponse> SetPartyAssistQuest(string accountId, JObject lazy)
        {
            var athenaData = await _mongoService.FindAthenaByAccountId(accountId);
            var profileChanges = new List<object>();

            profileChanges.Add(new StatModified
            {
                Name = "party_assist_quest",
                Value = lazy["questToPinAsPartyAssist"].ToString()
            });

            _mongoService.UpdateAthenaRvn(ref athenaData);
            return CreateProfileResponse(ref athenaData, profileChanges);
        }
    }
}
