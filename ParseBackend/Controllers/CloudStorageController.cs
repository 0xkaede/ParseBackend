﻿using Microsoft.AspNetCore.Mvc;
using ParseBackend.Exceptions;
using ParseBackend.Models.CloudStorage;
using ParseBackend.Models.Profile.Stats;
using ParseBackend.Services;
using System;
using System.Buffers;
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

        [HttpGet]
        [Route("cloudstorage/user/{accountId}")]
        public ActionResult<List<CloudstorageFile>> CloudStorageUser(string accountId)
        {
            bool exitsFile = DoesFortniteSettingsExist(accountId);

            if (!exitsFile)
                return new List<CloudstorageFile>();

            return new List<CloudstorageFile>()
            {
                new CloudstorageFile(GetFortniteSettingsPath(accountId), accountId)
            };
        }

        [HttpGet]
        [Route("cloudstorage/user/{accountId}/{fileName}")]
        public async Task CloudStorageSetttingsGet(string accountId, string fileName)
        {
            bool exitsFile = DoesFortniteSettingsExist(accountId);

            if (fileName != "ClientSettings.Sav" || !exitsFile)
                throw new BaseException("errors.com.epicgames.cloudstorage.file_not_found", "Settings not found!", 1904, "");


            Response.ContentType = "application/octet-stream";
            await Response.SendFileAsync(GetFortniteSettingsPath(accountId));
        }

        [HttpPut]
        [Route("cloudstorage/user/{accountId}/{fileName}")]
        public async Task<ActionResult> CloudStorageSetttingsPut(string accountId, string fileName)
        {
            using var reader = new StreamReader(HttpContext.Request.Body, Encoding.Latin1);
            var body = await reader.ReadToEndAsync();

            var path = SetingsPath + accountId;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            SaveFortniteSettings(accountId, body);

            StatusCode(204);
            return NoContent();
        }
    }
}
