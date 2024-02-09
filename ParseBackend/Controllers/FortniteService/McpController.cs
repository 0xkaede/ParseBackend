using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParseBackend.Exceptions;
using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Request;
using ParseBackend.Services;
using ParseBackend.Utils;
using System.Diagnostics;
using System.Text;

namespace ParseBackend.Controllers.FortniteService
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
            var sw = new Stopwatch();
            sw.Start();

            var body = string.Empty;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                body = await reader.ReadToEndAsync();

            _userService.SeeRequestData(JsonConvert.DeserializeObject<JObject>(body)!);

            var response = oparation switch
            {
                "QueryProfile" => await _userService.QueryProfile(profileId, accountId),
                "SetMtxPlatform" => await _userService.QueryProfile(profileId, accountId),
                "BulkEquipBattleRoyaleCustomization" => await _userService.QueryProfile(profileId, accountId),
                "EquipBattleRoyaleCustomization" => await _userService.EquipBattleRoyaleCustomization(accountId, JsonConvert.DeserializeObject<EquipBattleRoyaleCustomizationRequest>(body)!),
                "MarkItemSeen" => await _userService.MarkItemSeen(accountId, JsonConvert.DeserializeObject<MarkItemSeenRequest>(body)!),
                "SetItemFavoriteStatusBatch" => await _userService.SetItemFavoriteStatusBatch(accountId, JsonConvert.DeserializeObject<SetItemFavoriteStatusBatchRequest>(body)!),
                "ClientQuestLogin" => await _userService.ClientQuestLogin(accountId),
                "SetPartyAssistQuest" => await _userService.SetPartyAssistQuest(accountId, JsonConvert.DeserializeObject<JObject>(body)!),
                "PurchaseCatalogEntry" => await _userService.PurchaseCatalogEntry(accountId, JsonConvert.DeserializeObject<PurchaseCatalogEntryRequest>(body)!),
                "SetAffiliateName" => await _userService.SetAffiliateName(accountId, JsonConvert.DeserializeObject<JObject>(body)!),
                "GiftCatalogEntry" => await _userService.GiftCatalogEntry(accountId, JsonConvert.DeserializeObject<GiftCatalogEntryRequest>(body)!),
                "RemoveGiftBox" => await _userService.RemoveGiftBox(accountId, JsonConvert.DeserializeObject<RemoveGiftBoxResponse>(body)!),
                _ => throw new BaseException("", $"The action \"{oparation}\" was not found!", 1142, "MCP.Epic.Error")
            };

            sw.Stop();

            Logger.Log($"Did action {oparation} in {sw.ElapsedMilliseconds}ms");

            return response;
        }
    }
}
