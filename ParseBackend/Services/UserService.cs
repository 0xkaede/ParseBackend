using Jose;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParseBackend.Enums;
using ParseBackend.Exceptions;
using ParseBackend.Models.CUE4Parse.Challenges;
using ParseBackend.Models.Database.Athena;
using ParseBackend.Models.Database.CommonCore;
using ParseBackend.Models.Profile;
using ParseBackend.Models.Profile.Attributes;
using ParseBackend.Models.Profile.Changes;
using ParseBackend.Models.Request;
using ParseBackend.Models.Response;
using ParseBackend.Models.Storefront;
using ParseBackend.Utils;
using ParseBackend.Xmpp.Payloads;
using System;
using System.Net.WebSockets;
using System.Security.Authentication;
using static ParseBackend.Global;

namespace ParseBackend.Services
{
    public interface IUserService
    {
        public void SeeRequestData(JObject data);

        public Task<ProfileResponse> QueryProfile(string type, string accountId);
        public Task<ProfileResponse> EquipBattleRoyaleCustomization(string accountId, EquipBattleRoyaleCustomizationRequest body);
        public Task<ProfileResponse> MarkItemSeen(string accountId, MarkItemSeenRequest body);
        public Task<ProfileResponse> SetItemFavoriteStatusBatch(string accountId, SetItemFavoriteStatusBatchRequest body);
        public Task<ProfileResponse> ClientQuestLogin(string accountId);
        public Task<ProfileResponse> SetPartyAssistQuest(string accountId, JObject lazy);
        public Task<ProfileResponse> PurchaseCatalogEntry(string accountId, PurchaseCatalogEntryRequest body);
        public Task<ProfileResponse> SetAffiliateName(string accountId, JObject lazy);
        public Task<ProfileResponse> GiftCatalogEntry(string accountId, GiftCatalogEntryRequest body);
        public Task<ProfileResponse> RemoveGiftBox(string accountId, RemoveGiftBoxResponse body);
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

        public void SeeRequestData(JObject data)
        {
            Console.WriteLine(JsonConvert.SerializeObject(data, Formatting.Indented));
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

        public ProfileResponse CreateProfileResponse(ref CommonCoreData profile, List<object> profileChanges = null)
            => new ProfileResponse
            {
                ProfileRevision = profile.Rvn + 1,
                ProfileId = "common_core",
                ProfileChangesBaseRevisionRevision = profile.Rvn,
                ProfileChanges = profileChanges ?? new List<object>(),
                ProfileCommandRevision = profile.Rvn + 1,
                ServerTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                ResponseVersion = 1
            };

        public async Task<ProfileResponse> QueryProfile(string type, string accountId)
        {
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

            var profileChanges = new List<object>();

            if (body.VariantUpdates.Count() > 0)
            {
                foreach(var variantUpdate in body.VariantUpdates)
                {
                    var bHasStyle = athenaData.Items.FirstOrDefault(x => x.ItemIdResponse == body.ItemToSlot)!.Variants
                        .FirstOrDefault(x => x.Channel == variantUpdate.Channel)!.Owned.FirstOrDefault(x => x == variantUpdate.Active);

                    if (bHasStyle is null)
                        continue;

                    athenaData.Items.FirstOrDefault(x => x.ItemIdResponse == body.ItemToSlot)!.Variants
                        .FirstOrDefault(x => x.Channel == variantUpdate.Channel)!.Active = variantUpdate.Active;

                    _mongoService.UpdateAthenaItemVariants(ref athenaData, body.ItemToSlot);

                    profileChanges.Add(new ItemAttrChanged
                    {
                        ItemId = body.ItemToSlot,
                        AttributeName = "variants",
                        AttributeValue = athenaData.Items.FirstOrDefault(x => x.ItemIdResponse == body.ItemToSlot)!.Variants,
                    });
                }
            }

            _mongoService.EquipAthenaItem(ref athenaData, body.SlotName, body.ItemToSlot, body.IndexWithinSlot);

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

            _mongoService.UpdateAthenaQuestAssist(ref athenaData, lazy["questToPinAsPartyAssist"]!.ToString());

            profileChanges.Add(new StatModified
            {
                Name = "party_assist_quest",
                Value = athenaData.Stats.QuestAssist
            });

            _mongoService.UpdateAthenaRvn(ref athenaData);
            return CreateProfileResponse(ref athenaData, profileChanges);
        }

        public async Task<ProfileResponse> PurchaseCatalogEntry(string accountId, PurchaseCatalogEntryRequest body)
        {
            var athenaData = await _mongoService.FindAthenaByAccountId(accountId);
            var commonCoreData = await _mongoService.FindCommonCoreByAccountId(accountId);

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

            catalog.Purchace(ref commonCoreData);

            foreach (var item in catalog.ItemGrants)
            {
                var variant = await _fileProviderService.GetCosmeticsVariants(item.TemplateId.Contains(":") ? item.TemplateId.Split(":")[1] : item.TemplateId);

                _mongoService.AddedAthenaItem(ref athenaData, new AthenaItemsData
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

            _mongoService.UpdateCommonCoreVbucks(ref commonCoreData);
            _mongoService.UpdateAthenaRvn(ref athenaData);
            _mongoService.UpdateCommonCoreRvn(ref commonCoreData);

            return new ProfileResponse
            {
                ProfileRevision = commonCoreData.Rvn + 1,
                ProfileId = "common_core",
                ProfileChangesBaseRevisionRevision = commonCoreData.Rvn,
                ProfileChanges = profileChanges ?? new List<object>(),
                ProfileCommandRevision = commonCoreData.Rvn + 1,
                ServerTime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                MultiUpdate = new List<ProfileResponse>
                {
                    new ProfileResponse
                    {
                         ProfileRevision = athenaData.Rvn + 1,
                         ProfileId = "athena",
                         ProfileChangesBaseRevisionRevision = athenaData.Rvn,
                         ProfileChanges = multiUpdateEnded ?? new List<object>(),
                         ProfileCommandRevision = athenaData.Rvn + 1,
                    }
                },
                Notifications = notifications,
                ResponseVersion = 1
            };
        }

        public async Task<ProfileResponse> SetAffiliateName(string accountId, JObject lazy)
        {
            var commonCoreData = await _mongoService.FindCommonCoreByAccountId(accountId);
            var profileChanges = new List<object>();

            _mongoService.UpdateSac(ref commonCoreData, lazy["affiliateName"]!.ToString());

            profileChanges.Add(new StatModified
            {
                Name = "mtx_affiliate_set_time",
                Value = commonCoreData.Stats.MtxAffiliateTime.TimeToString()
            });

            profileChanges.Add(new StatModified
            {
                Name = "mtx_affiliate",
                Value = commonCoreData.Stats.MtxAffiliate
            });

            _mongoService.UpdateCommonCoreRvn(ref commonCoreData);
            return CreateProfileResponse(ref commonCoreData, profileChanges);
        }

        public async Task<ProfileResponse> GiftCatalogEntry(string accountId, GiftCatalogEntryRequest body)
        {
            var commonCoreData = await _mongoService.FindCommonCoreByAccountId(accountId);
            var profileChanges = new List<object>();

            body.ValidateRequest(commonCoreData.Stats.GiftRemaining, true);

            var friendsData = await _mongoService.FindFriendsByAccountId(accountId);
            var friendAccept = friendsData.List.Where(x => x.Status is FriendsStatus.Accepted).ToList();

            foreach (var reciverId in body.ReceiverAccountIds)
            {
                var data = friendAccept.FirstOrDefault(x => x.AccountId == reciverId);

                if(data is null)
                    throw new BaseException("errors.com.epicgames.friends.no_friendship", $"{reciverId} isnt friends with you!", 140537, "");
            }

            var catalog = await _fileProviderService.GetCatalogByOfferId(body.OfferId);
            if(catalog is null)
                throw new BaseException("errors.com.epicgames.catalog.not_found", $"Offer Id \"{body.OfferId}\" was not found!", 18304, "");

            profileChanges = catalog.Purchace(ref commonCoreData, body.ReceiverAccountIds);

            foreach(var reciverId in body.ReceiverAccountIds)
            {
                var athena = await _mongoService.FindAthenaByAccountId(reciverId);
                var common = await _mongoService.FindCommonCoreByAccountId(reciverId);

                if(!common.Stats.ReciveGifts)
                    throw new BaseException("errors.com.epicgames.gift.gift_disabled", $"User \"{reciverId}\" has gifts disabled!", 30125, "");

                foreach(var item in catalog.ItemGrants)
                {
                    var findIt = athena.Items.FirstOrDefault(x => x.ItemId == item.TemplateId);

                    if(findIt != null)
                        throw new BaseException("errors.com.epicgames.item.owened", $"User \"{reciverId}\" already owns the items!", 13532, "");
                }
            }

            foreach (var reciverId in body.ReceiverAccountIds)
            {
                var athena = await _mongoService.FindAthenaByAccountId(reciverId);
                var common = await _mongoService.FindCommonCoreByAccountId(reciverId);

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

                foreach(var item in catalog.ItemGrants)
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

                    _mongoService.AddedAthenaItem(ref athena, new AthenaItemsData
                    {
                        Seen = false,
                        Amount = item.Quantity,
                        IsFavorite = false,
                        ItemId = item.TemplateId,
                        ItemIdResponse = item.TemplateId.ComputeSHA256Hash(),
                        Variants = variants,
                    });

                    var bad = JObject.FromObject(giftBoxItem.Attributes).ToObject<GiftBoxAttribute>();

                    bad.LootList.Add(new GiftBoxLootList
                    {
                        ItemType = profileItem.TemplateId,
                        ItemGuid = giftBoxItemId,
                        ItemProfile = "athena",
                        Quantity = 1
                    });

                    giftBoxItem.Attributes = bad;
                }

                _mongoService.AddedCommonCoreGift(ref common, new CommonCoreDataGifts
                {
                    FromAccountId = accountId,
                    LootList = catalog.ItemGrants.Select(x => x.TemplateId).ToList(),
                    TemplateId = body.GiftWrapTemplateId,
                    TemplateIdHashed = giftBoxItemId,
                    Time = DateTime.UtcNow,
                    UserMessage = body.PersonalMessage
                });

                if(reciverId == accountId)
                {
                    profileChanges.Add(new ItemAdded
                    {
                        Item = giftBoxItem,
                        ItemId = giftBoxItemId,
                    });
                }

                _mongoService.UpdateCommonCoreRvn(ref common);
                _mongoService.UpdateAthenaRvn(ref athena);

                var client = GlobalXmppServer.FindClientFromAccountId(reciverId);
                if(client != null)
                {
                    client.SendMessage(JsonConvert.SerializeObject(new PayLoad<object>
                    {
                        Payload = new object(),
                        Timestamp = CurrentTime(),
                        Type = "com.epicgames.gift.received"
                    }));
                }
            }

            _mongoService.UpdateCommonCoreRvn(ref commonCoreData);
            return CreateProfileResponse(ref commonCoreData, profileChanges);
        }

        public async Task<ProfileResponse> RemoveGiftBox(string accountId, RemoveGiftBoxResponse body)
        {
            var commonCoreData = await _mongoService.FindCommonCoreByAccountId(accountId);
            var profileChanges = new List<object>();

            var delete = commonCoreData.Gifts.FirstOrDefault(x => x.TemplateIdHashed == body.GiftBoxItemIds);

            if (delete != null)
            {
                commonCoreData.Gifts.Remove(delete);

                profileChanges.Add(new ItemRemoved
                {
                    ItemId = body.GiftBoxItemIds,
                });
            }

            _mongoService.UpdateCommonCoreGift(accountId, commonCoreData.Gifts);
            _mongoService.UpdateCommonCoreRvn(ref commonCoreData);
            return CreateProfileResponse(ref commonCoreData, profileChanges);
        }
    }
}
