using Microsoft.AspNetCore.Mvc;

namespace ParseBackend.Controllers
{
    [ApiController]
    [Route("/waitingroom/api/waitingroom")]
    public class WaitingRoomController : Controller
    {
        public ActionResult WatingRoom()
        {
            Response.StatusCode = 204;
            return NoContent();
        }
    }
}
