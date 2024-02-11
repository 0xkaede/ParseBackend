using ParseBackend.Enums;
using ParseBackend.Exceptions;
using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Profile.Attributes;
using ParseBackend.Models.FortniteService.Profile;
using ParseBackend.Models.FortniteService.Request;
using ParseBackend.Models.Other.Cache;
using static ParseBackend.Global;
using Newtonsoft.Json.Linq;
using ParseBackend.Models.Other.Database.Athena;
using MongoDB.Driver;
using ParseBackend.Models.FortniteService.Profile.Changes;
using ParseBackend.Xmpp.Payloads;
using Newtonsoft.Json;
using ParseBackend.Models.Other.Database.CommonCore;

namespace ParseBackend.Controllers.FortniteService.Mcp
{
    public sealed partial class McpController
    {
        private async Task<ProfileResponse> GiftCatalogEntryAction(ProfileCache profiles, GiftCatalogEntryRequest body)
        {
            var profileChanges = new List<object>();
            var accountId = profiles.UserData.AccountId;

            body.ValidateRequest(profiles.CommonData.Stats.GiftRemaining, true);

            var friendsData = await _mongoService.ReadFriendsData(accountId);
            var friendAccept = friendsData.List.Where(x => x.Status is FriendsStatus.Accepted).ToList();

            foreach (var reciverId in body.ReceiverAccountIds)
            {
                var data = friendAccept.FirstOrDefault(x => x.AccountId == reciverId);

                if (data is null)
                    throw new BaseException("errors.com.epicgames.friends.no_friendship", $"{reciverId} isnt friends with you!", 140537, "");
            }

            var catalog = await _fileProviderService.GetCatalogByOfferId(body.OfferId);
            if (catalog is null)
                throw new BaseException("errors.com.epicgames.catalog.not_found", $"Offer Id \"{body.OfferId}\" was not found!", 18304, "");

            profiles.CommonData.PurchaceCatalog(catalog);

            foreach (var reciverId in body.ReceiverAccountIds)
            {
                var reciverProfiles = await _mongoService.GetAllProfileData(reciverId);

                if (!reciverProfiles.CommonData.Stats.ReciveGifts)
                    throw new BaseException("errors.com.epicgames.gift.gift_disabled", $"User \"{reciverId}\" has gifts disabled!", 30125, "");

                foreach (var item in catalog.ItemGrants)
                {
                    var findIt = reciverProfiles.AthenaData.Items.FirstOrDefault(x => x.ItemId == item.TemplateId);

                    if (findIt != null)
                        throw new BaseException("errors.com.epicgames.item.owened", $"User \"{reciverId}\" already owns the items!", 13532, "");
                }
            }

            foreach (var reciverId in body.ReceiverAccountIds)
            {
                var reciverProfiles = await _mongoService.GetAllProfileData(reciverId);

                var giftBoxItemId = CreateUuid();
                var giftBoxItem = new ProfileItem
                {
                    TemplateId = body.GiftWrapTemplateId,
                    Attributes = new GiftBoxAttribute
                    {
                        FromAccountId = accountId,
                        LootList = new List<GiftBoxLootList>(),
                        Params = new Dictionary<string, string>
                        {
                            {
                                "userMessage",
                                body.PersonalMessage
                            }
                        },
                        Level = 1,
                        GiftedOn = CurrentTime()
                    },
                    Quantity = 1,
                };

                foreach (var item in catalog.ItemGrants)
                {
                    var variants = await _fileProviderService.GetCosmeticsVariants(item.TemplateId.Contains(":") ? item.TemplateId.Split(":")[1] : item.TemplateId);

                    var profileItem = new ProfileItem
                    {
                        TemplateId = item.TemplateId,
                        Attributes = new ItemAttributes
                        {
                            ItemSeen = false,
                            Favorite = false,
                            RandomSelectionCount = 0,
                            Level = -1,
                            MaxLevelBonus = 0,
                            Variants = variants,
                            XP = 0,
                        },
                        Quantity = item.Quantity
                    };

                    reciverProfiles.AthenaData.Items.Add(new AthenaItemsData
                    {
                        Seen = false,
                        Amount = item.Quantity,
                        IsFavorite = false,
                        ItemId = item.TemplateId,
                        ItemIdResponse = item.TemplateId.ComputeSHA256Hash(),
                        Variants = variants,
                    });

                    var bad = JObject.FromObject(giftBoxItem.Attributes).ToObject<GiftBoxAttribute>();

                    bad!.LootList.Add(new GiftBoxLootList
                    {
                        ItemType = profileItem.TemplateId,
                        ItemGuid = giftBoxItemId,
                        ItemProfile = "athena",
                        Quantity = 1
                    });

                    giftBoxItem.Attributes = bad;
                }

                profiles.CommonData.Gifts.Add(new CommonCoreDataGifts
                {
                    FromAccountId = accountId,
                    LootList = catalog.ItemGrants.Select(x => x.TemplateId).ToList(),
                    TemplateId = body.GiftWrapTemplateId,
                    TemplateIdHashed = giftBoxItemId,
                    Time = DateTime.UtcNow,
                    UserMessage = body.PersonalMessage
                });

                if (reciverId == accountId)
                {
                    profileChanges.Add(new ItemAdded
                    {
                        Item = giftBoxItem,
                        ItemId = giftBoxItemId,
                    });
                }

                profiles.AthenaData.Rvn += 1;
                profiles.CommonData.Rvn += 1;

                var client = GlobalXmppServer.FindClientFromAccountId(reciverId);
                if (client != null)
                {
                    client.SendMessage(JsonConvert.SerializeObject(new PayLoad<object>
                    {
                        Payload = new object(),
                        Timestamp = CurrentTime(),
                        Type = "com.epicgames.gift.received"
                    }));
                }
            }

            return profiles.CommonData.CreateMcpResponse(profileChanges);
        }
    }
}
