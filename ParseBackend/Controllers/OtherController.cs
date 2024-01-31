using Microsoft.AspNetCore.Mvc;

namespace ParseBackend.Controllers
{
    [ApiController]
    public class OtherController : Controller
    {
        [HttpPost]
        [Route("datarouter/api/v1/public/data")]
        public ActionResult Datarouter()
        {
            StatusCode(204);
            return Content("");
        }
    }
}
