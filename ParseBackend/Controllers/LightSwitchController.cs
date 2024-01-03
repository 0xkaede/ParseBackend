using Microsoft.AspNetCore.Mvc;
using ParseBackend.Models.Response;
using ParseBackend.Services;
using ParseBackend.Utils;

namespace ParseBackend.Controllers
{
    [ApiController]
    [Route("/lightswitch/api/service")]
    public class LightSwitchController : Controller
    {
        private readonly IMongoService _mongoService;

        public LightSwitchController(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        [HttpGet]
        [Route("Fortnite/status")]
        public async Task<ActionResult<FortniteStatusResponse>> FortniteStatus()
        {
            var token = ContextUtils.VerifyToken(HttpContext); // this say user will have a message saying "You have been banned from fortnite"

            var user = await _mongoService.FindUserByAccountId(token.Iai);

            return new FortniteStatusResponse
            {
                ServiceInstanceId = "fortnite",
                Status = "UP",
                Message = "Fortniteisonline",
                MaintenanceUri = null,
                OverrideCatalogIds = new List<string> { "a7f138b2e51945ffbfdacc1af0541053" },
                AllowedActions = new List<string>(),
                Banned = user.BannedData.IsBanned,
                LauncherInfoDTO = new LauncherInfoDTO
                {
                    AppName = "Fortnite",
                    CatalogItemId = "4fe75bbc5a674f4f9b356b5c90567da5",
                    Namespace = "fn"
                }
            };
        }

        [HttpGet]
        [Route("bulk/status")]
        public async Task<ActionResult<FortniteStatusResponse>> BulkStatus()
        {
            var token = ContextUtils.VerifyToken(HttpContext); // this say user will have a message saying "You have been banned from fortnite"

            var user = await _mongoService.FindUserByAccountId(token.Iai);

            return new FortniteStatusResponse
            {
                ServiceInstanceId = "fortnite",
                Status = "UP",
                Message = "fortniteisup",
                MaintenanceUri = null,
                OverrideCatalogIds = new List<string> { "a7f138b2e51945ffbfdacc1af0541053" },
                AllowedActions = new List<string> { "PLAY", "DOWNLOAD" },
                Banned = user.BannedData.IsBanned,
                LauncherInfoDTO = new LauncherInfoDTO
                {
                    AppName = "Fortnite",
                    CatalogItemId = "4fe75bbc5a674f4f9b356b5c90567da5",
                    Namespace = "fn"
                }
            };
        }
    }
}
