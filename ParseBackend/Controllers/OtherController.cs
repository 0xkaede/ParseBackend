using Microsoft.AspNetCore.Mvc;
using ParseBackend.Models.Other;
using ParseBackend.Services;

namespace ParseBackend.Controllers
{
    [ApiController]
    public class OtherController : Controller
    {
        private readonly IMongoService _mongoService;

        public OtherController(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        [HttpPost]
        [Route("datarouter/api/v1/public/data")]
        public ActionResult Datarouter()
        {
            StatusCode(204);
            return Content("");
        }

        [HttpGet]
        [Route("affiliate/api/public/affiliates/slug/{slugId}")]
        public async Task<ActionResult<SlugResponse>> GetAffiliates(string slugId)
        {
            var codeInfo = await _mongoService.FindSacByCode(slugId);

            if (codeInfo is null)
            {
                Response.StatusCode = 404;
                return null;
            }

            var userData = await _mongoService.FindUserByAccountId(codeInfo.AccountId);

            return new SlugResponse(codeInfo.Code, userData.Username);
        }
    }
}
