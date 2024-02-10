using Microsoft.AspNetCore.Mvc;
using ParseBackend.Models.LightSwitchService;
using ParseBackend.Services;
using ParseBackend.Utils;

namespace ParseBackend.Controllers.LightSwitchService
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
        public async Task<ActionResult<FortniteStatus>> FortniteStatus()
        {
            var token = ContextUtils.VerifyToken(HttpContext); // this say user will have a message saying "You have been banned from fortnite"

            var user = await _mongoService.ReadUserData(token.Iai);

            return new FortniteStatus
            {
                ServiceInstanceId = "fortnite",
                Status = "UP",
                Message = "Fortniteisonline",
                MaintenanceUri = null,
                OverrideCatalogIds = new List<string> { "a7f138b2e51945ffbfdacc1af0541053" },
                AllowedActions = new List<string>(),
                Banned = user.BannedData.Type is Enums.BannedType.Perm or Enums.BannedType.Hwid ? true : false, //ig
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
        public async Task<ActionResult<List<FortniteStatus>>> BulkStatus()
        {
            var token = ContextUtils.VerifyToken(HttpContext); // this say user will have a message saying "You have been banned from fortnite"

            var user = await _mongoService.ReadUserData(token.Iai);

            return new List<FortniteStatus>()
            {
                new FortniteStatus
                {
                    ServiceInstanceId = "fortnite",
                    Status = "UP",
                    Message = "fortniteisup",
                    MaintenanceUri = null,
                    OverrideCatalogIds = new List<string> { "a7f138b2e51945ffbfdacc1af0541053" },
                    AllowedActions = new List<string> { "PLAY", "DOWNLOAD" },
                    Banned = user.BannedData.Type is Enums.BannedType.Perm or Enums.BannedType.Hwid ? true : false,
                    LauncherInfoDTO = new LauncherInfoDTO
                    {
                        AppName = "Fortnite",
                        CatalogItemId = "4fe75bbc5a674f4f9b356b5c90567da5",
                        Namespace = "fn"
                    }
                },
            };
        }
    }
}
