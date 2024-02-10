using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Profile.Changes;
using ParseBackend.Models.FortniteService.Request;
using ParseBackend.Models.Other.Cache;

namespace ParseBackend.Controllers.FortniteService.Mcp
{
    public sealed partial class McpController
    {
        private ProfileResponse MarkItemSeen(ProfileCache profiles, MarkItemSeenRequest body)
        {
            var profileChanges = new List<object>();

            foreach (var item in body.ItemIds)
            {
                profiles.AthenaData.SeeItem(item);

                profileChanges.Add(new ItemAttrChanged
                {
                    ItemId = item,
                    AttributeName = "item_seen",
                    AttributeValue = true
                });
            }

            return profiles.AthenaData.CreateMcpResponse(profileChanges);
        }
    }
}
