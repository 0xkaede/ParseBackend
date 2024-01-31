using Microsoft.AspNetCore.Mvc;
using ParseBackend.Models.Response.Accounts;
using ParseBackend.Services;
using ParseBackend.Utils;

namespace ParseBackend.Controllers
{
    [ApiController]
    [Route("account/api/public/account")]
    public class AccountController : Controller //might do later on idk
    {
        private readonly IMongoService _mongoService;

        public AccountController(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AccountPublicResponse>>> AccountReturn([FromQuery] string accountId)
        {
            //ContextUtils.VerifyToken(HttpContext);

            var data = await _mongoService.FindUserByAccountId(accountId);

            return new List<AccountPublicResponse>()
            {
                new AccountPublicResponse
                {
                    DisplayName = data.Username,
                    Id = accountId
                }
            };
        }

        [HttpGet]
        [Route("{accountId}")]
        public async Task<ActionResult<AccountInfoResponse>> AccountInfoData(string accountId)
        {
            //ContextUtils.VerifyToken(HttpContext);

            var accountInfo = await _mongoService.FindUserByAccountId(accountId);
            return new AccountInfoResponse
            {
                Id = accountInfo.AccountId,
                DisplayName = accountInfo.Username,
                Name = "Parse",
                Email = $"[hidden]{accountInfo.Email.Split("@")[1]}",
                FailedLoginAttempts = 0,
                LastLogin = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ"),
                NumberOfDisplayNameChanges = 0,
                AgeGroup = "UNKNOWN",
                Headless = false,
                Country = "US",
                LastName = "Backend",
                PreferredLanguage = "en",
                CanUpdateDisplayName = false,
                TfaEnabled = false,
                EmailVerified = true,
                MinorVerified = false,
                MinorStatus = "NOT_MINOR",
                CabinedMode = false,
                HasHashedEmail = false
            };
        }

        [HttpGet]
        [Route("{accountId}/externalAuths")]
        public ActionResult<object> ExternalAuths(string accountId) => new object();

        [HttpGet]
        [Route("displayName/{displayName}")]
        public async Task<ActionResult<AccountPublicResponse>> GetUserDisplayName(string displayName)
        {
            var data = await _mongoService.FindUserByAccountName(displayName);

            return new AccountPublicResponse
            {
                DisplayName = data.Username,
                Id = data.AccountId,
                ExternalAuths = new List<string>(),
            };
        }
    }
}
