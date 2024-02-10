using ParseBackend.Exceptions;
using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Profile;
using ParseBackend.Models.FortniteService.Profile.Attributes;
using ParseBackend.Models.FortniteService.Profile.Changes;
using ParseBackend.Models.FortniteService.Request;
using ParseBackend.Models.Other.Cache;
using ParseBackend.Models.Other.Database.Athena;
using ParseBackend.Models.Other.Database.CommonCore;
using static MongoDB.Driver.WriteConcern;

namespace ParseBackend.Controllers.FortniteService.Mcp
{
    public sealed partial class McpController
    {
        private async Task<ProfileResponse> PurchaseCatalogEntryAction(ProfileCache profiles, PurchaseCatalogEntryRequest body)
        {
            var catalog = await _fileProviderService.GetCatalogByOfferId(body.OfferId);
            if (catalog is null)
                throw new BaseException("errors.com.epicgames.modules.catalog", $"The offerId \"{body.OfferId}\" could not be found!", 1424, "");

            var profileChanges = new List<object>();
            var notifications = new List<NotificationsResponse>
            {
                new NotificationsResponse
                {
                    Type = "CatalogPurchase",
                    Primary = true,
                    NotificationLoots = new List<NotificationLoot>()
                }
            };
            var multiUpdateEnded = new List<object>();

            profiles.CommonData.PurchaceCatalog(catalog);

            foreach (var item in catalog.ItemGrants)
            {
                var variant = await _fileProviderService.GetCosmeticsVariants(item.TemplateId.Contains(":") ? item.TemplateId.Split(":")[1] : item.TemplateId);

                profiles.AthenaData.Items.Add(new AthenaItemsData
                {
                    Seen = false,
                    Amount = item.Quantity,
                    IsFavorite = false,
                    ItemId = item.TemplateId,
                    ItemIdResponse = item.TemplateId.ComputeSHA256Hash(),
                    Variants = variant, //add later today
                });

                multiUpdateEnded.Add(new ItemAdded
                {
                    Item = new ProfileItem
                    {
                        TemplateId = item.TemplateId,
                        Quantity = item.Quantity,
                        Attributes = new ItemAttributes
                        {
                            ItemSeen = false,
                            Favorite = false,
                            Level = -1,
                            MaxLevelBonus = 0,
                            RandomSelectionCount = 0,
                            Variants = variant,
                            XP = 0,
                        }
                    },
                    ItemId = item.TemplateId.ComputeSHA256Hash(),
                });

                notifications[0].NotificationLoots.Add(new NotificationLoot()
                {
                    ItemType = item.TemplateId,
                    ItemGuid = item.TemplateId.ComputeSHA256Hash(),
                    ItemProfile = "athena",
                    Quantity = 1
                });
            }

            profiles.AthenaData.Rvn += 1;
            profiles.CommonData.Rvn += 1;

            return new ProfileResponse
            {
                ProfileRevision = profiles.CommonData.Rvn + 1,
                ProfileId = "common_core",
                ProfileChangesBaseRevisionRevision = profiles.CommonData.Rvn,
                ProfileChanges = profileChanges ?? new List<object>(),
                ProfileCommandRevision = profiles.CommonData.Rvn + 1,
                ServerTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                MultiUpdate = new List<ProfileResponse>
                {
                    new ProfileResponse
                    {
                         ProfileRevision = profiles.AthenaData.Rvn + 1,
                         ProfileId = "athena",
                         ProfileChangesBaseRevisionRevision = profiles.AthenaData.Rvn,
                         ProfileChanges = multiUpdateEnded ?? new List<object>(),
                         ProfileCommandRevision = profiles.AthenaData.Rvn + 1,
                    }
                },
                Notifications = notifications,
                ResponseVersion = 1
            };
        }
    }
}
