using Microsoft.AspNetCore.Mvc;
using ParseBackend.Exceptions;
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
        private readonly IUserService _userService;

        public McpController(IMongoService mongoService, IUserService userService)
        {
            _mongoService = mongoService;
            _userService = userService;

            //ContextUtils.VerifyToken(HttpContext); //Middle
        }

        [HttpPost]
        [Route("{accountId}/client/{oparation}")]
        public async Task<ActionResult<ProfileResponse>> McpPost([FromQuery] string profileId, [FromQuery] int rvn, string accountId, string oparation)
        {
            Logger.Log(oparation);
            var response = oparation.ToLower() switch
            {
                "queryprofile" => await _userService.QueryProfile(profileId, accountId),
                "setmtxplatform" => await _userService.QueryProfile(profileId, accountId),
                "bulkequipbattleroyalecustomization" => await _userService.QueryProfile(profileId, accountId),
                _ => throw new BaseException("", $"The action \"{oparation}\" was not found!", 1142, "MCP.Epic.Error")
            };
            
            return response;
        }
    }
}
