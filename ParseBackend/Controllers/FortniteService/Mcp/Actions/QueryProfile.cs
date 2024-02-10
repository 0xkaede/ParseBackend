using Microsoft.AspNetCore.Mvc;
using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Profile;
using ParseBackend.Models.FortniteService.Profile.Changes;
using ParseBackend.Models.Other.Cache;

namespace ParseBackend.Controllers.FortniteService.Mcp
{
    public sealed partial class McpController
    {
        private async Task<ProfileResponse> QueryProfileAction(ProfileCache profile, string profileType)
        {
            var profileRes = profileType switch
            {
                "athena" => profile.AthenaData.CreateFortniteProfile(),
                "common_core" => profile.CommonData.CreateFortniteProfile(profile.UserData),
                "common_public" => await _mongoService.CreateCommonPublicProfile(profile.UserData.AccountId),
                _ => new Profile(),
            };

            var profileChanges = new List<object>();

            profileChanges.Add(new FullProfileUpdate // what was i doing
            {
                Profile = profileRes
            });

            return profileRes.QueryProfile();
        }
    }
}
