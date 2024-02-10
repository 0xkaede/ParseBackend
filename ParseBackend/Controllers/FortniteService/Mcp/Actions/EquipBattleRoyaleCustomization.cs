using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Profile.Changes;
using ParseBackend.Models.FortniteService.Request;
using ParseBackend.Models.Other.Cache;

namespace ParseBackend.Controllers.FortniteService.Mcp
{
    public sealed partial class McpController
    {
        private ProfileResponse EquipBattleRoyaleCustomizationAction(ProfileCache profiles, EquipBattleRoyaleCustomizationRequest body)
        {
            var profileChanges = new List<object>();

            if (body.VariantUpdates.Count() > 0)
            {
                foreach (var variantUpdate in body.VariantUpdates)
                {
                    var bHasStyle = profiles.AthenaData.Items.FirstOrDefault(x => x.ItemIdResponse == body.ItemToSlot)!.Variants
                        .FirstOrDefault(x => x.Channel == variantUpdate.Channel)!.Owned.FirstOrDefault(x => x == variantUpdate.Active) is null;

                    if (variantUpdate.Channel == "Numeric")
                        bHasStyle = false;

                    if (bHasStyle)
                        continue;

                    profiles.AthenaData.Items.FirstOrDefault(x => x.ItemIdResponse == body.ItemToSlot)!.Variants
                        .FirstOrDefault(x => x.Channel == variantUpdate.Channel)!.Active = variantUpdate.Active;
                }

                profileChanges.Add(new ItemAttrChanged
                {
                    ItemId = body.ItemToSlot,
                    AttributeName = "variants",
                    AttributeValue = profiles.AthenaData.Items.FirstOrDefault(x => x.ItemIdResponse == body.ItemToSlot)!.Variants,
                });
            }

            var changesValue = profiles.AthenaData.EquipItem(body.ItemToSlot, body.SlotName, body.IndexWithinSlot);
            var changesName = body.SlotName.ToLower().Contains("wrap") ? "itemwraps" : body.SlotName.ToLower();

            if (body.SlotName is "Dance")
                changesValue = profiles.AthenaData.Stats.CurrentItems.CurrentEmotes;
            else if(body.SlotName is "ItemWrap")
                changesValue = profiles.AthenaData.Stats.CurrentItems.CurrentWraps;

            var data = new StatModified
            {
                Name = $"favorite_{changesName}",
                Value = changesValue
            };

            profileChanges.Add(data);

            return profiles.AthenaData.CreateMcpResponse(profileChanges);
        }
    }
}
