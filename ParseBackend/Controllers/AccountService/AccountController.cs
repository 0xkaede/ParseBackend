using Microsoft.AspNetCore.Mvc;
using ParseBackend.Enums.Other;
using ParseBackend.Models.AccountService;
using ParseBackend.Services;
using ParseBackend.Utils;

namespace ParseBackend.Controllers.AccountService
{
    [ApiController]
    [Route("account/api/public/account")]
    public class AccountController : Controller
    {
        private readonly IMongoService _mongoService;

        public AccountController(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<AccountInfoPublic>>> AccountReturn([FromQuery] string accountId)
        {
            //ContextUtils.VerifyToken(HttpContext);

            var data = await _mongoService.ReadUserData(accountId);

            return new List<AccountInfoPublic>()
            {
                new AccountInfoPublic
                {
                    DisplayName = data.Username,
                    Id = accountId
                }
            };
        }

        [HttpGet]
        [Route("{accountId}")]
        public async Task<ActionResult<AccountInfo>> AccountInfoData(string accountId)
        {
            //ContextUtils.VerifyToken(HttpContext);

            var accountInfo = await _mongoService.ReadUserData(accountId);
            
            return new AccountInfo
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
        public async Task<ActionResult<AccountInfoPublic>> GetUserDisplayName(string displayName)
        {
            var data = await _mongoService.ReadUserData(displayName, DatabaseSearchType.Username);

            return new AccountInfoPublic
            {
                DisplayName = data.Username,
                Id = data.AccountId,
                ExternalAuths = new List<string>(),
            };
        }
    }
}
