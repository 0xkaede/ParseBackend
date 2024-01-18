using Microsoft.AspNetCore.Mvc;
using ParseBackend.Models.CloudStorage;
using ParseBackend.Services;
using System.Text;
using static ParseBackend.Global;

namespace ParseBackend.Controllers
{
    [ApiController]
    [Route("fortnite/api")]
    public class CloudStorageController : Controller
    {
        private readonly IMongoService _mongoService;

        public CloudStorageController(IMongoService mongoService)
        {
            _mongoService = mongoService;
        }

        [HttpGet]
        [Route("cloudstorage/system")]
        public ActionResult<List<CloudstorageFile>> CloudStorageSystem()
        {
            return Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\CloudStorage").Select(x => new CloudstorageFile(x)).ToList();
        }

        [HttpGet]
        [Route("cloudstorage/system/{file}")]
        public async Task<ActionResult<string>> CloudStorageFile(string file)
        {
            return System.IO.File.ReadAllText($"{Directory.GetCurrentDirectory()}\\CloudStorage\\{file}");
        }

        [HttpGet]
        [Route("cloudstorage/system/config")]
        public ActionResult<List<object>> CloudStorageSystemConfig()
        {
            return new List<object>();
        }
    }
}
