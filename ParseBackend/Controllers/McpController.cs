using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParseBackend.Exceptions;
using ParseBackend.Models.Request;
using ParseBackend.Models.Response;
using ParseBackend.Services;
using ParseBackend.Utils;
using System.Text;

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

            var body = string.Empty;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                body = await reader.ReadToEndAsync();


            var response = oparation.ToLower() switch
            {
                "queryprofile" => await _userService.QueryProfile(profileId, accountId),
                "setmtxplatform" => await _userService.QueryProfile(profileId, accountId),
                "bulkequipbattleroyalecustomization" => await _userService.QueryProfile(profileId, accountId),
                "equipbattleroyalecustomization" => await _userService.EquipBattleRoyaleCustomization(accountId, JsonConvert.DeserializeObject<EquipBattleRoyaleCustomizationRequest>(body)!),
                "markitemseen" => await _userService.MarkItemSeen(accountId, JsonConvert.DeserializeObject<MarkItemSeenRequest>(body)!),
                "SetItemFavoriteStatusBatch" => await _userService.SetItemFavoriteStatusBatch(accountId, JsonConvert.DeserializeObject<SetItemFavoriteStatusBatchRequest>(body)!),
                _ => throw new BaseException("", $"The action \"{oparation}\" was not found!", 1142, "MCP.Epic.Error")
            }; ;
            
            return response;
        }
    }
}
