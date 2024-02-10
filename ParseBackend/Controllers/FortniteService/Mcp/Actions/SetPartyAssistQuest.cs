using Newtonsoft.Json.Linq;
using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Profile.Changes;
using ParseBackend.Models.Other.Cache;

namespace ParseBackend.Controllers.FortniteService.Mcp
{
    public sealed partial class McpController
    {
        private ProfileResponse SetPartyAssistQuestAction(ProfileCache profiles, JObject body)
        {
            var profileChanges = new List<object>();

            profiles.AthenaData.Stats.QuestAssist = body["questToPinAsPartyAssist"]!.ToString();

            profileChanges.Add(new StatModified
            {
                Name = "party_assist_quest",
                Value = profiles.AthenaData.Stats.QuestAssist
            });

            return profiles.AthenaData.CreateMcpResponse(profileChanges);
        }
    }
}
