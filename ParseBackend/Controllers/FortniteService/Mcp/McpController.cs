using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ParseBackend.Exceptions;
using ParseBackend.Models.FortniteService;
using ParseBackend.Models.FortniteService.Request;
using ParseBackend.Models.Other.Database.Athena;
using ParseBackend.Services;
using ParseBackend.Utils;
using System.Diagnostics;
using System.Text;

namespace ParseBackend.Controllers.FortniteService.Mcp
{
    [ApiController]
    [Route("fortnite/api/game/v2/profile")]
    public sealed partial class McpController : Controller
    {
        private readonly IMongoService _mongoService;
        private readonly IFileProviderService _fileProviderService;

        public McpController(IMongoService mongoService, IFileProviderService fileProviderService)
        {
            _mongoService = mongoService;
            _fileProviderService = fileProviderService;

            //ContextUtils.VerifyToken(HttpContext); //Middle
        }

        [HttpPost]
        [Route("{accountId}/client/{oparation}")]
        public async Task<ActionResult<ProfileResponse>> Client([FromQuery] string profileId, string accountId, string oparation)
        {
            var sw = new Stopwatch();
            sw.Start();

            var body = string.Empty;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                body = await reader.ReadToEndAsync();

            //_userService.SeeRequestData(JsonConvert.DeserializeObject<JObject>(body)!);

            var profiles = await _mongoService.GetAllProfileData(accountId);

            var res = oparation switch
            {
                "QueryProfile" => await QueryProfileAction(profiles, profileId),
                "SetMtxPlatform" => await QueryProfileAction(profiles, profileId),
                "BulkEquipBattleRoyaleCustomization" => await QueryProfileAction(profiles, profileId),
                "ClientQuestLogin" => await ClientQuestLoginActionAsync(profiles),
                "EquipBattleRoyaleCustomization" => EquipBattleRoyaleCustomizationAction(profiles, JsonConvert.DeserializeObject<EquipBattleRoyaleCustomizationRequest>(body)!),
                "MarkItemSeen" => MarkItemSeen(profiles, JsonConvert.DeserializeObject<MarkItemSeenRequest>(body)!),
                "SetItemFavoriteStatusBatch" => SetItemFavoriteStatusBatchAction(profiles, JsonConvert.DeserializeObject<SetItemFavoriteStatusBatchRequest>(body)!),
                "SetPartyAssistQuest" => SetPartyAssistQuestAction(profiles, JsonConvert.DeserializeObject<JObject>(body)!),
                "PurchaseCatalogEntry" => await PurchaseCatalogEntryAction(profiles, JsonConvert.DeserializeObject<PurchaseCatalogEntryRequest>(body)!),
                "GiftCatalogEntry" => await GiftCatalogEntryAction(profiles, JsonConvert.DeserializeObject<GiftCatalogEntryRequest>(body)!),
                "RemoveGiftBox" => RemoveGiftBoxAction(profiles, JsonConvert.DeserializeObject<RemoveGiftBoxResponse>(body)!),
                _ => throw new BaseException("", $"The action \"{oparation}\" was not found!", 1142, "MCP.Epic.Error")
            };

            sw.Stop();

            Logger.Log($"Did action {oparation} in {sw.ElapsedMilliseconds}ms");

            return res;
        }
    }
}
