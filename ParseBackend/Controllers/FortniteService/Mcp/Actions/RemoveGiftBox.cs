using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Profile.Changes;
using ParseBackend.Models.FortniteService.Request;
using ParseBackend.Models.Other.Cache;

namespace ParseBackend.Controllers.FortniteService.Mcp
{
    public sealed partial class McpController
    {
        public ProfileResponse RemoveGiftBoxAction(ProfileCache profiles, RemoveGiftBoxResponse body)
        {
            var profileChanges = new List<object>();

            var delete = profiles.CommonData.Gifts.FirstOrDefault(x => x.TemplateIdHashed == body.GiftBoxItemIds);

            if (delete != null)
            {
                profiles.CommonData.Gifts.Remove(delete);

                profileChanges.Add(new ItemRemoved
                {
                    ItemId = body.GiftBoxItemIds,
                });
            }

            profiles.CommonData.Rvn += 1;

            return profiles.CommonData.CreateMcpResponse(profileChanges);
        }
    }
}
