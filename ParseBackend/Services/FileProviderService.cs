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
using System.Net;
using System.IO.Pipelines;
using Newtonsoft.Json;
using ParseBackend.Models.Storefront;
using ParseBackend.Models.Other;
using CUE4Parse.GameTypes.PUBG.Assets.Exports;
using ParseBackend.Models.Profile.Attributes;
using CUE4Parse.UE4.Objects.GameplayTags;

namespace ParseBackend.Services
{
    public interface IFileProviderService
    {
        public List<string> GetAllCosmetics();

        public Task<Dictionary<string, BaseChallenge>> GenerateDailyQuest(List<string> questAssets = null);
        public Task<Catalog> GenerateItemShop();

        public Task<CatalogEntry> GetCatalogByOfferId(string offerId);
        public Task<List<Variant>> GetCosmeticsVariants(string itemId);
    }

    public class FileProviderService : IFileProviderService
    {
        public static DefaultFileProvider Provider { get; set; }

        private List<string> CosmeticItemsCache = new List<string>();

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

        public List<string> GetAllCosmetics()
        {
            if(CosmeticItemsCache.Count != 0)
                return CosmeticItemsCache;

            var data = GetAssetsFromPath($"FortniteGame/Content/Athena/Items/Cosmetics");

            CosmeticItemsCache = data;

            return data;
        }

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

        private async Task<Dictionary<string, int>> GetChallengesObjectives(UObject questData) //might be universal already idk tho
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

        private async Task<Dictionary<string, int>> GetChallengesRewards(UObject questData)
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

        public async Task<CatalogEntry> GetCatalogByOfferId(string offerId)
        {
            foreach (var dailyStoreFront in Config.DailyStoreFront)
                if (offerId == dailyStoreFront.ItemIds[0].ComputeSHA256Hash())
                    return await GenerateCatalogEntry(dailyStoreFront);

            foreach (var weeklyStoreFront in Config.WeeklyStoreFront)
                if (offerId == weeklyStoreFront.ItemIds[0].ComputeSHA256Hash())
                    return await GenerateCatalogEntry(weeklyStoreFront, 1);

            return null;
        }

        public async Task<Catalog> GenerateItemShop()
        {
            var dailyShop = new List<CatalogEntry>();
            var weeklyShop = new List<CatalogEntry>();

            foreach (var dailyStoreFront in Config.DailyStoreFront)
                dailyShop.Add(await GenerateCatalogEntry(dailyStoreFront));

            foreach (var weeklyStoreFront in Config.WeeklyStoreFront)
                weeklyShop.Add(await GenerateCatalogEntry(weeklyStoreFront, 1));

            return new Catalog
            {
                DailyPurchaseHrs = 24,
                Expiration = "9999-12-31T00:00:00.000Z",
                RefreshIntervalHrs = 24,
                Storefronts = new List<Storefront>
                {
                    new Storefront
                    {
                        Name = "BRDailyStorefront",
                        CatalogEntries = dailyShop
                    },
                    new Storefront
                    {
                        Name = "BRWeeklyStorefront",
                        CatalogEntries = weeklyShop
                    }
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type">0 = Daily, 1 = Weekly Config</param>
        /// <returns></returns>
        private async Task<CatalogEntry> GenerateCatalogEntry(StorefrontConfiguration data, int type = 0)
        {
            var cosmetics = GetAllCosmetics();

            var itemGrants = new List<CatalogEntryItemGrant>();
            var requirements = new List<CatalogEntryRequirements>();

            var displayAsset = string.Empty;

            foreach (var item in data.ItemIds)
            {
                var findItem = cosmetics.FirstOrDefault(x => x.ToLower().Contains(item.ToLower()));

                if (findItem is null)
                    return new CatalogEntry();

                var itemObject = await Provider.LoadObjectAsync(findItem.SubstringBefore("."));

                var itemType = itemObject.ExportType.Replace("ItemDefinition", string.Empty);

                if (itemObject.TryGetValue(out FSoftObjectPath displayAssetPath, "DisplayAssetPath"))
                    if (string.IsNullOrEmpty(displayAsset))
                        displayAsset = displayAssetPath.AssetPathName.PlainText;

                itemGrants.Add(new CatalogEntryItemGrant
                {
                    Quantity = 1,
                    TemplateId = $"{itemType}:{itemObject.Name}"
                });

                requirements.Add(new CatalogEntryRequirements
                {
                    MinQuantity = 1,
                    RequiredId = $"{itemType}:{itemObject.Name}",
                    RequirementType = "DenyOnItemOwnership",
                });
            }

            var metaData = new List<CatalogEntryMetaInfo>
            {
                new CatalogEntryMetaInfo
                {
                    Key = "SectionId",
                    Value = type is 0 ? "Featured" : "Featured2",
                },
                new CatalogEntryMetaInfo
                {
                    Key = "TileSize",
                    Value = type is 0 ? "Small" : "Normal"
                },
            };

            if (!string.IsNullOrEmpty(data.BannerOverride))
            {
                metaData.Add(new CatalogEntryMetaInfo
                {
                    Key = "BannerOverride",
                    Value = data.BannerOverride
                });
            }


            return new CatalogEntry
            {
                DevName = $"[Dev]Kaede:{"ItemShop".ComputeSHA256Hash()}:{data.ItemIds[0].ComputeSHA256Hash()}",
                OfferId = data.ItemIds[0].ComputeSHA256Hash(),
                FulfillmentIds = { },
                DailyLimit = -1,
                WeeklyLimit = -1,
                MonthlyLimit = -1,
                Categories = new List<string> { data.CategoryNumber },
                Prices = new List<CatalogEntryPrice>
                {
                    new CatalogEntryPrice
                    {
                        CurrencyType = "MtxCurrency",
                        CurrencySubType = "",
                        RegularPrice = data.Price,
                        FinalPrice = data.Price,
                        SaleExpiration = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                        BasePrice = data.Price
                    }
                },
                Meta = "{\"SectionId\":\"Featured\",\"TileSize\":\"Small\"}",
                MatchFilter = "",
                FilterWeight = 0,
                AppStoreId = new List<string>(),
                Requirements = requirements,
                OfferType = "StaticPrice",
                GiftInfo = new CatalogEntryGiftInfo
                {
                    ForcedGiftBoxTemplateId = "",
                    IsEnabled = true,
                    GiftRecordIds = { },
                    PurchaseRequirements = { },
                },
                Refundable = false,
                MetaInfo = metaData,
                DisplayAssetPath = type is 1 ? displayAsset : "",
                ItemGrants = itemGrants,
                SortPriority = -1,
                CatalogGroupPriority = 0,
            };
        }

        public async Task<List<Variant>> GetCosmeticsVariants(string itemId)
        {
            var response = new List<Variant>();
            var items = GetAllCosmetics();

            var findItem = items.FirstOrDefault(x => x.ToLower().Contains(itemId.ToLower()));

            if (findItem is null)
                return new List<Variant>();

            var itemObject = await Provider.LoadObjectAsync(findItem.SubstringBefore("."));

            if(!itemObject.TryGetValue(out UObject[] itemVariants, "ItemVariants"))
                return new List<Variant>();

            foreach(var variant in itemVariants)
            {
                variant.TryGetValue(out FGameplayTag variantChannelTag, "VariantChannelTag");

                if (variant.ExportType is "FortCosmeticNumericalVariant") //hard
                {
                    response.Add(new Variant
                    {
                        Channel = variantChannelTag.GetLastTag(),
                        Active = "1",
                        Owned = new List<string>(),
                    });
                }

                if(variant.TryGetValue(out FStructFallback[] partOptions, "PartOptions"))
                    response.Add(GenerateVariantFromStruct(partOptions, variantChannelTag.GetLastTag()));

                if (variant.TryGetValue(out FStructFallback[] materialOptions, "MaterialOptions"))
                    response.Add(GenerateVariantFromStruct(materialOptions, variantChannelTag.GetLastTag()));
            }

            return response;
        }

        private Variant GenerateVariantFromStruct(FStructFallback[] options, string tag)
        {
            var variant = new Variant
            {
                Channel = tag,
                Active = "",
                Owned = new List<string>(),
            };

            foreach (var option in options)
            {
                option.TryGetValue(out FGameplayTag customizationVariantTag, "CustomizationVariantTag");
                option.TryGetValue(out bool bIsDefault, "bIsDefault");

                if (bIsDefault)
                    variant.Active = customizationVariantTag.GetLastTag();

                variant.Owned.Add(customizationVariantTag.GetLastTag());
            }

            return variant;
        }
    }
}
