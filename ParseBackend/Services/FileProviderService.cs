using ParseBackend.Utils;
using System.Xml.Linq;
using System;
using static ParseBackend.Global;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.UE4.Versions;
using ParseBackend.Enums;
using ParseBackend.Models.Provider;
using LogLevel = ParseBackend.Utils.LogLevel;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Objects.UObject;
using CUE4Parse.Utils;
using CUE4Parse.UE4.Assets.Objects;
using ParseBackend.Models.CUE4Parse.Challenges;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.UE4.Assets.Exports.Engine;

namespace ParseBackend.Services
{
    public interface IFileProviderService
    {
        public List<string> GetAllCosmetics();

        public Task<Dictionary<string, BaseChallenge>> GenerateDailyQuest(List<string> questAssets = null);
    }

    public class FileProviderService : IFileProviderService
    {
        public static DefaultFileProvider Provider { get; set; }

        public FileProviderService()
        {
            try
            {
                var providerInfo = GetProviderInfo();
                Provider = new DefaultFileProvider(Config.GamePath, SearchOption.TopDirectoryOnly, false, new VersionContainer(providerInfo.EGame));
                Provider.Initialize();

                var keys = new List<KeyValuePair<FGuid, FAesKey>>
                {
                    new KeyValuePair<FGuid, FAesKey>(new FGuid(), new FAesKey(providerInfo.MainAes))
                };

                Provider.SubmitKeys(keys);

                Logger.Log($"File provider initalized with {Provider.Keys.Count} keys", LogLevel.CUE4Parse);

                foreach (var file in Provider.MountedVfs)
                    Logger.Log($"Mounted File: {file.Name}", LogLevel.CUE4Parse);

            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.Error);
            }
        }

        private ProviderInfo GetProviderInfo() => Config.FortniteVersions switch
        {
            FortniteVersions.Version_11_31 => new ProviderInfo(EGame.GAME_UE4_24, "0x6C51ABA88CA1240A0D14EB94701F6C41FD7799B102E9060D1E6C316993196FDF"),
        };

        private List<string> GetAssetsFromPath(string path)
            => Provider.Files.Where(x => x.Key.ToLower().StartsWith(path.ToLower())
            && x.Key.EndsWith(".uasset")).Select(x => x.Key).ToList();

        public List<string> GetAllCosmetics() => GetAssetsFromPath($"FortniteGame/Content/Athena/Items/Cosmetics");

        public async Task<Dictionary<string, BaseChallenge>> GenerateDailyQuest(List<string> questAssets = null)
        {
            var response = new Dictionary<string, BaseChallenge>();
            var grantQuestsList = questAssets ?? new List<string>();

            if(questAssets is null)
            {
                var allQuestsList = GetAssetsFromPath("FortniteGame/Content/Athena/Items/Quests/DailyQuests/Quests");

                var random = new Random();

                for (int i = 0; i < 3; i++) //the only way ik icl
                {
                    int index = random.Next(allQuestsList.Count);
                    var newQuest = allQuestsList[index];

                    allQuestsList.Remove(newQuest);
                    grantQuestsList.Add(newQuest);
                }
            }

            foreach (var quest in grantQuestsList)
            {
                var questData = await Provider.LoadObjectAsync(quest.SubstringBefore("."));

                var challengeObjects = await GetChallengesObjectives(questData);
                var challengeRewards = await GetChallengesRewards(questData);

                var data = new BaseChallenge
                {
                    Rewards = challengeRewards,
                    Objects = challengeObjects,
                };

                response.Add(quest.SubstringAfterLast("/").SubstringBefore("."), data);
            }

            return response;
        }

        public async Task<Dictionary<string, int>> GetChallengesObjectives(UObject questData) //might be universal already idk tho
        {
            questData.TryGetValue(out FStructFallback[] questObjectives, "Objectives");

            var data = new Dictionary<string, int>();

            foreach (var obj in questObjectives)
            {
                obj.TryGetValue(out FName backendName, "BackendName");
                obj.TryGetValue(out int count, "Count");

                data.Add(backendName.PlainText, count);
            }

            return data;
        }

        public async Task<Dictionary<string, int>> GetChallengesRewards(UObject questData)
        {
            var ver = Config.FortniteVersions;
            var data = new Dictionary<string, int>();

            if (questData.TryGetValue(out FStructFallback[] hiddenRewards, "HiddenRewards")) //need if statement as not everyone has this
            {
                foreach (var obj in hiddenRewards)
                {
                    obj.TryGetValue(out FName templateId, "TemplateId");
                    obj.TryGetValue(out int quantity, "Quantity");
                    data.Add(templateId.PlainText, quantity);
                }
            }

            if (questData.TryGetValue(out UDataTable rewardsTable, "RewardsTable")) //need if statement as not everyone has this
            {
                foreach (var item in rewardsTable.RowMap)
                {
                    item.Value.TryGetValue(out FName templateId, "TemplateId");
                    item.Value.TryGetValue(out int quantity, "Quantity");
                    data.Add(templateId.PlainText, quantity);
                }
            }

            //need 1 more i thinkg butttttttt i dont have 8.51 installed

            return data;
        }
    }
}
