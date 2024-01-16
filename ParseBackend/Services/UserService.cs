using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ParseBackend.Exceptions;
using ParseBackend.Models.Database.Athena;
using ParseBackend.Models.Profile;
using ParseBackend.Models.Profile.Changes;
using ParseBackend.Models.Request;
using ParseBackend.Models.Response;
using ParseBackend.Utils;
using System.Security.Authentication;

namespace ParseBackend.Services
{
    public interface IUserService
    {
        public Task<ProfileResponse> QueryProfile(string type, string accountId);
        public Task<ProfileResponse> EquipBattleRoyaleCustomization(string accountId, JObject request);
        public Task<ProfileResponse> MarkItemSeen(string accountId, JObject request);
    }

    public class UserService : IUserService
    {
        private readonly IMongoService _mongoService;

        public UserService(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        public async Task<ProfileResponse> CreateProfileResponse(Profile profile, List<object> profileChanges = null)
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

        public async Task<ProfileResponse> CreateProfileResponse(AthenaData profile, List<object> profileChanges = null)
            => new ProfileResponse
            {
                ProfileRevision = profile.Rvn,
                ProfileId = "athena",
                ProfileChangesBaseRevisionRevision = profile.Rvn -1,
                ProfileChanges = profileChanges ?? new List<object>(),
                ProfileCommandRevision = profile.Rvn,
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

            profileChanges.Add(JObject.FromObject(new FullProfileUpdate
            {
                Profile = profile
            }));

            return await CreateProfileResponse(profile, profileChanges);
        }

        public async Task<ProfileResponse> EquipBattleRoyaleCustomization(string accountId, JObject request)
        {
            var body = request.ToObject<EquipBattleRoyaleCustomizationRequest>();

            var athenaData = await _mongoService.FindAthenaByAccountId(accountId);

            _mongoService.EquipAthenaItem(ref athenaData, body.SlotName, body.ItemToSlot);

            var profileChanges = new List<object>();

            profileChanges.Add(new StatModified
            {
                Name = $"favorite_{body.SlotName.ToLower()}",
                Value = body.ItemToSlot.ToString()
            });

            return await CreateProfileResponse(athenaData, profileChanges);
        }

        public async Task<ProfileResponse> MarkItemSeen(string accountId, JObject request)
        {
            var body = request.ToObject<MarkItemSeenRequest>();

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

            return await CreateProfileResponse(athenaData, profileChanges);
        }
    }
}
