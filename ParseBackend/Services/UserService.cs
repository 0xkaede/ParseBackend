using Newtonsoft.Json.Linq;
using ParseBackend.Models.Profile;
using ParseBackend.Models.Profile.Changes;
using ParseBackend.Models.Response;
using ParseBackend.Utils;
using System.Security.Authentication;

namespace ParseBackend.Services
{
    public interface IUserService
    {
        public Task<ProfileResponse> QueryProfile(string type, string accountId);
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
    }
}
