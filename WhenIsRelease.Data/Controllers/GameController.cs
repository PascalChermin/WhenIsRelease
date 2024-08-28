using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using WhenIsRelease.Data.Business;
using WhenIsRelease.Data.Scheduler;
using WhenIsRelease.Models;
using WhenIsRelease.Models.GameReleases;

namespace WhenIsRelease.Data.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly ISocial _twitter;
        private readonly IGameBusiness _gameBusiness;

        public GameController(ILogger<GameController> logger, IBusiness gameBusiness, ISocial twitter)
        {
            _logger = logger;
            _twitter = twitter;
            _gameBusiness = (IGameBusiness)gameBusiness;
        }

        [HttpGet("{id}")]
        public IRelease GetRelease(int id)
        {
            _logger.LogTrace($"Started function {nameof(GetRelease)}");

            try
            {
                return _gameBusiness.GetRelease(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(GetRelease)}");
                return null;
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(GetRelease)}");
            }
        }

        [HttpGet("ical")]
        public ActionResult GetReleaseCalendar([FromQuery] int[] regions, [FromQuery] int[] platforms)
        {
            _logger.LogTrace($"Started function {nameof(GetReleaseCalendar)}");

            try
            {
                var iCal = _gameBusiness.GetReleaseCalendar(new List<int[]> { regions, platforms });
                byte[] calendarBytes = System.Text.Encoding.UTF8.GetBytes(iCal);

                return File(calendarBytes, "text/calendar", "event.ics");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(GetReleaseCalendar)}");
                return null;
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(GetReleaseCalendar)}");
            }
        }

        [HttpGet]
        public List<IRelease> GetReleases([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            _logger.LogTrace($"Started function {nameof(GetRelease)}");

            try
            {
                return _gameBusiness.GetReleases(startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(GetRelease)}");
                return null;
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(GetRelease)}");
            }
        }

        [HttpGet]
        [Route("allplatforms")]
        public List<Platform> GetAllPlatforms()
        {
            _logger.LogTrace($"Started function {nameof(GetAllPlatforms)}");

            try
            {
                return _gameBusiness.GetAllPlatforms();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(GetAllPlatforms)}");
                return null;
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(GetAllPlatforms)}");
            }
        }

        [HttpGet]
        [Route("platforms")]
        public List<Platform> GetPlatforms()
        {
            _logger.LogTrace($"Started function {nameof(GetPlatforms)}");

            try
            {
                return _gameBusiness.GetPlatforms();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(GetPlatforms)}");
                return null;
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(GetPlatforms)}");
            }
        }

        [HttpGet]
        [Route("regions")]
        public List<Region> GetRegions()
        {
            _logger.LogTrace($"Started function {nameof(GetRegions)}");

            try
            {
                return _gameBusiness.GetRegions();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(GetRegions)}");
                return null;
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(GetRegions)}");
            }
        }

        [HttpGet]
        [Route("update")]
        public ActionResult UpdateDatabase()
        {
            _logger.LogTrace($"Started function {nameof(UpdateDatabase)}");

            try
            {
                _gameBusiness.UpdateDatabase();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(UpdateDatabase)}");
                return null;
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(UpdateDatabase)}");
            }
        }

        [HttpGet]
        [Route("tweet")]
        public ActionResult TweetReleases()
        {
            _logger.LogTrace($"Started function {nameof(TweetReleases)}");

            try
            {
                _twitter.PublishReleasesOfTheDay(_gameBusiness.GetReleases(DateTime.Today, DateTime.Today.AddDays(1)));
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(TweetReleases)}");
                return null;
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(TweetReleases)}");
            }
        }

        [HttpGet("search/{query}")]
        public List<IRelease> SearchReleases(string query, [FromQuery(Name = "regions")] int[] regions, [FromQuery(Name = "platforms")] int[] platforms)
        {
            _logger.LogTrace($"Started function {nameof(SearchReleases)}");

            try
            {
                return _gameBusiness.Search(query, regions, platforms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(SearchReleases)}");
                return null;
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(SearchReleases)}");
            }
        }
    }
}