using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Profile.Changes;
using ParseBackend.Models.FortniteService.Request;
using ParseBackend.Models.Other.Cache;
using ParseBackend.Models.Other.Database.Athena;

namespace ParseBackend.Controllers.FortniteService.Mcp
{
    public sealed partial class McpController
    {
        private ProfileResponse SetItemFavoriteStatusBatchAction(ProfileCache profiles, SetItemFavoriteStatusBatchRequest body)
        {
            var profileChanges = new List<object>();

            for (var i = 0; i < body.ItemIds.Count(); i++)
            {
                var templateId = body.ItemIds[i];
                var status = body.ItemFavStatus[i];

                profiles.AthenaData.FavoriteItem(templateId, status);

                profileChanges.Add(new ItemAttrChanged
                {
                    ItemId = body.ItemIds[i],
                    AttributeName = "favorite",
                    AttributeValue = body.ItemFavStatus[i]
                });
            }

            return profiles.AthenaData.CreateMcpResponse(profileChanges);
        }
    }
}
