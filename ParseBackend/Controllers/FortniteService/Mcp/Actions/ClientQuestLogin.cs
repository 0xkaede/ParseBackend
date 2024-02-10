using Newtonsoft.Json.Linq;
using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Profile.Attributes;
using ParseBackend.Models.FortniteService.Profile.Changes;
using ParseBackend.Models.FortniteService.Profile;
using ParseBackend.Models.Other.Cache;
using static ParseBackend.Global;

namespace ParseBackend.Controllers.FortniteService.Mcp
{
    public sealed partial class McpController
    {
        private async Task<ProfileResponse> ClientQuestLoginActionAsync(ProfileCache profiles)
        {
            var profileChanges = new List<object>();

            if(profiles.AthenaData.CheckQuestLogin())
            {
                var questList = await _fileProviderService.GenerateDailyQuest();

                profiles.AthenaData.CheckAndUpdateRerolls();

                profiles.AthenaData.UpdateQuests(questList);
            }

            foreach (var quest in profiles.AthenaData.DailyQuestData.Quests)
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

            return profiles.AthenaData.CreateMcpResponse(profileChanges);
        }
    }
}
