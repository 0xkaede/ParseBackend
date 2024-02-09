using Microsoft.AspNetCore.Mvc;
using ParseBackend.Models.Response;
using ParseBackend.Services;

namespace ParseBackend.Controllers
{
    [ApiController]
    public class MatchmakingController : Controller
    {
        private readonly IMongoService _mongoService;

        public MatchmakingController(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        [HttpGet]
        [Route("/fortnite/api/game/v2/matchmakingservice/ticket/player/{accoundId}")]
        public async Task<ActionResult<FortniteStatusResponse>> FortniteStatus(string accountId)
        {
            return null;
        }
    }
}
