using Microsoft.AspNetCore.Mvc;
using ParseBackend.Models.Response;
using ParseBackend.Services;
using ParseBackend.Utils;

namespace ParseBackend.Controllers
{
    [ApiController]
    [Route("fortnite/api/game/v2/profile")]
    public class McpController : Controller
    {
        private readonly IMongoService _mongoService;

        public McpController(IMongoService mongoService)
        {
            _mongoService = mongoService;

            ContextUtils.VerifyToken(HttpContext);
        }

        [HttpPost]
        [Route("{accountId}/client/ClientQuestLogin")]
        [Route("{accountId}/client/QueryProfile")]
        [Route("{accountId}/client/SetMtxPlatform")]
        [Route("{accountId}/client/BulkEquipBattleRoyaleCustomization")]
        public async Task<ActionResult<ProfileResponse>> QueryProfile([FromQuery] string profileId, [FromQuery] int rvn, string accountId, string action)
        {
            return null;
        }
    }
}
