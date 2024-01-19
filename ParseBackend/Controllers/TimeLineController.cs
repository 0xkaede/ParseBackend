using Microsoft.AspNetCore.Mvc;
using ParseBackend.Models.Calendar.States;
using ParseBackend.Models.Calendar;
using static ParseBackend.Global;

namespace ParseBackend.Controllers
{
    [ApiController]
    [Route("/fortnite/api/calendar/v1/timeline")]
    public class TimeLineController : Controller
    {
        public ActionResult<Timeline> Timeline()
        {
            return new Timeline
            {
                CacheIntervalMinutes = 10,
                Channels = new Dictionary<string, TimelineChannel>()
                {
                    {
                        "client-matchmaking", new TimelineChannel
                        {
                            States = new List<ChannelState>(),
                            CacheExpire = "9999-01-01T00:00:00.000Z",
                        }
                    },
                    {
                        "client-events", new TimelineChannel
                        {
                            States = new List<ChannelState>()
                            {
                                new ChannelState
                                {
                                    ValidFrom = "0001-01-01T00:00:00.000Z",
                                    ActiveEvents = new List<ChannelEvent>
                                    {
                                        new ChannelEvent
                                        {
                                            EventType = $"EventFlag.Season{Config.FortniteSeason}",
                                            ActiveUntil = "9999-01-01T00:00:00.000Z",
                                            ActiveSince = "2020-01-01T00:00:00.000Z"
                                        },
                                        new ChannelEvent
                                        {
                                            EventType = $"EventFlag.{Config.FortniteSeason}",
                                            ActiveUntil = "9999-01-01T00:00:00.000Z",
                                            ActiveSince = "2020-01-01T00:00:00.000Z"
                                        }
                                    },
                                    State = new ClientEventsState
                                    {
                                        ActiveStorefronts = new List<string>(),
                                        EventNamedWeights = new Dictionary<string, double>(),
                                        SeasonNumber = Config.FortniteSeason,
                                        SeasonTemplateId = $"AthenaSeason:athenaseason{Config.FortniteSeason}",
                                        MatchXpBonusPoints = 0,
                                        SeasonBegin = "2020-01-01T00:00:00Z",
                                        SeasonEnd = "9999-01-01T00:00:00Z",
                                        SeasonDisplayedEnd = "9999-01-01T00:00:00Z",
                                        WeeklyStoreEnd = "2023-08-05T23:59:00.000Z",
                                        StwEventStoreEnd = "9999-01-01T00:00:00.000Z",
                                        StwWeeklyStoreEnd = "9999-01-01T00:00:00.000Z",
                                        SectionStoreEnds = new Dictionary<string, string>()
                                        {
                                            {
                                                "Featured",
                                                "2023-08-05T23:59:00.000Z"
                                            }
                                        },
                                        DailyStoreEnd = "2023-08-05T23:59:00.000Z"
                                    }
                                }
                            },
                            CacheExpire = "9999-01-01T00:00:00.000Z"
                        }
                    }
                },

                CurrentTime = CurrentTime(),
            };
        }
    }
}
