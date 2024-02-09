using Microsoft.AspNetCore.Mvc;
using ParseBackend.Models.LightSwitchService;
using ParseBackend.Services;

namespace ParseBackend.Controllers.FortniteService
{
    [ApiController]
    [Route("fortnite/api/game/v2")]
    public class MatchmakingController : Controller
    {
        private readonly IMongoService _mongoService;

        public MatchmakingController(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        [HttpGet]
        [Route("matchmakingservice/ticket/player/{accoundId}")]
        public async Task<ActionResult<FortniteStatus>> FortniteStatus(string accountId)
        {
            return null;
        }
    }
}
