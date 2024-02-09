using Microsoft.AspNetCore.Mvc;
using static ParseBackend.Global;
using ParseBackend.Models.FortniteService.Content;

namespace ParseBackend.Controllers.FortniteService
{
    [ApiController]
    [Route("content/api/pages")]
    public class ContentPagesController : Controller
    {
        [HttpGet]
        [Route("fortnite-game")]
        public async Task<ActionResult<BasePagesEntry>> FortniteGame()
        {
            var stage = $"season{Config.FortniteSeason}";

            if (Config.FortniteVersions is Enums.FortniteVersions.Version_11_31)
                stage = "winter19";

            return new Pages
            {
                LastModified = "2019-11-01T17:33:35.346Z",
                ActiveDate = "2017-08-30T03:20:48.050Z",
                Title = "Fortnite Game",
                BattleRoyaleNews = new BattleRoyaleNewsEntry
                {
                    ActiveDate = "2018-08-17T16:00:00.000Z",
                    LastModified = "2019-10-31T20:29:39.334Z",
                    Title = "battleroyalenews",
                    News = new BattleRoyaleNews
                    {
                        Type = "Battle Royale News",
                        MessagesOfTheDay = new List<object>(), //neeed todo
                        Messages = new List<Message>
                        {
                            new Message
                            {
                                Title = "Welcome to Parse Backend",
                                Body = "Created by 0xkaede",
                                Spotlight = true,
                                Hidden = false,
                                Image = "https://cdn.discordapp.com/attachments/1126068951684759633/1137763721297539082/download.jpg",
                                Type = "CommonUI Simple Message Base",
                            }
                        }
                    }
                },
                EmergencyNotice = new EmergencyNotice
                {
                    ActiveDate = "2018-08-06T19:00:26.217Z",
                    LastModified = "2019-10-29T22:32:52.686Z",
                    Title = "emergencynotice",
                    News = new BattleRoyaleNews
                    {
                        Messages = new List<Message>
                        {
                            new Message
                            {
                                Hidden = false,
                                Spotlight = true,
                                Body = "Backend Created by 0xkaede",
                                Title = "Welcome to Parse Backend",
                                Type = "CommonUI Simple Message Base",
                            }
                        }
                    }
                },
                DynamicBackgrounds = new DynamicBackgrounds
                {
                    ActiveDate = "2018-08-06T19:00:26.217Z",
                    LastModified = "2019-10-29T22:32:52.686Z",
                    Title = "dynamicbackgrounds",
                    Backgrounds = new DynamicBackgroundList
                    {
                        Type = "DynamicBackgroundList",
                        Backgrounds = new List<DynamicBackground>
                        {
                            new DynamicBackground
                            {
                                Key = "lobby",
                                Stage = stage,
                                Type = "DynamicBackground"
                            },
                            new DynamicBackground
                            {
                                Key = "vault",
                                Stage = stage,
                                Type = "DynamicBackground"
                            }
                        }
                    }
                }
            };
        }
    }
}
