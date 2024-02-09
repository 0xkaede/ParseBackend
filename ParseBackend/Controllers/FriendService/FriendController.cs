using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ParseBackend.Models.FriendService;
using ParseBackend.Services;
using static ParseBackend.Global;

namespace ParseBackend.Controllers.FriendService
{
    [ApiController]
    [Route("friends/api")]
    public class FriendsController : Controller
    {
        private readonly IMongoService _mongoService;
        private readonly IFriendService _friendService;

        public FriendsController(IMongoService mongoService, IFriendService friendService)
        {
            _mongoService = mongoService;
            _friendService = friendService;
        }

        [HttpGet]
        [Route("v1/{accountId}/settings")]
        public ActionResult GetSettings(string accountId)
        {
            return Content("{}");
        }

        [HttpGet]
        [Route("v1/{accountId}/blocklist")]
        [Route("public/list/fortnite/{accountId}/recentPlayers")]
        public ActionResult GetBlockList(string accountId)
        {
            return Content("[]");
        }

        [HttpGet]
        [Route("public/friends/{accountId}")]
        public async Task<ActionResult<List<FriendOld>>> GetFriends(string accountId)
        {
            var friends = await _mongoService.FindFriendsByAccountId(accountId);
            var res = new List<FriendOld>();

            foreach (var friend in friends.List)
                res.Add(new FriendOld(friend));

            return res;
        }

        [HttpGet]
        [Route("public/blocklist/{accountId}")]
        public async Task<ActionResult<JObject>> GetBlocks(string accountId)
        {
            var friends = await _mongoService.FindFriendsByAccountId(accountId);

            var blocked = friends.List.Where(x => x.Status is Enums.FriendsStatus.Blocked);

            var list = new List<string>();

            foreach (var friend in blocked)
                list.Add(friend.AccountId);

            var res = JObject.FromObject(new { blockedUsers = list });

            return res;
        }

        [HttpGet]
        [Route("v1/{accountId}/summary")]
        public async Task<ActionResult<FriendSummary>> GetSummary(string accountId)
        {
            var res = new FriendSummary();

            var friends = await _mongoService.FindFriendsByAccountId(accountId);

            return res;
        }

        [HttpPost]
        [Route("v1/{accountId}/friends/{receiverId}")]
        [Route("v1/friends/{accountId}/{receiverId}")]
        [Route("public/friends/{accountId}/{receiverId}")]
        public async Task<ActionResult<string>> PostFriends(string accountId, string receiverId)
        {
            var sender = await _mongoService.FindFriendsByAccountId(accountId);
            var receiver = await _mongoService.FindFriendsByAccountId(receiverId);

            if (sender.List.Where(x => x.Status is Enums.FriendsStatus.Incoming)
                .FirstOrDefault(x => x.AccountId == receiver.AccountId) != null)
                await _friendService.AcceptFriendRequest(sender.AccountId, receiver.AccountId);

            if (sender.List.Where(x => x.Status is Enums.FriendsStatus.Outgoing)
                .FirstOrDefault(x => x.AccountId == receiver.AccountId) is null)
                await _friendService.SendFriendRequest(sender.AccountId, receiver.AccountId);

            Response.StatusCode = 403;
            return "";
        }

        [HttpDelete]
        [Route("v1/{accountId}/friends/{receiverId}")]
        [Route("v1/friends/{accountId}/{receiverId}")]
        [Route("public/friends/{accountId}/{receiverId}")]
        public async Task<ActionResult<string>> DeleteFriends(string accountId, string receiverId)
        {
            var sender = await _mongoService.FindFriendsByAccountId(accountId);
            var receiver = await _mongoService.FindFriendsByAccountId(receiverId);

            if (!await _friendService.DeleteFriend(sender.AccountId, receiver.AccountId))
            {
                Response.StatusCode = 403;
                return "";
            }


            Response.StatusCode = 403;
            return "";
        }
    }
}
