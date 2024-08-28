using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WhenIsRelease.Frontend.Services;
using WhenIsRelease.Models;
using WhenIsRelease.Models.GameReleases;

namespace WhenIsRelease.Frontend
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameServiceController : ControllerBase
    {
        private readonly GameService _service;

        public GameServiceController(GameService service)
        {
            _service = service;
        }

        [HttpGet("regions")]
        public async Task<IEnumerable<Region>> GetRegionsAsync()
        {
            try
            {
                return await _service.GetRegions();
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        [HttpGet("allplatforms")]
        public async Task<IEnumerable<Platform>> GetAllPlatformsAsync()
        {
            try
            {
                return await _service.GetAllPlatforms();
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        [HttpGet("platforms")]
        public async Task<IEnumerable<Platform>> GetPlatformsAsync()
        {
            try
            {
                return await _service.GetPlatforms();
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }
        
        [HttpGet("search/{query}")]
        public async Task<IEnumerable<IRelease>> SearchReleasesAsync(string query, [FromQuery(Name = "regions")] int[] regions, [FromQuery(Name = "platforms")] int[] platforms)
        {
            try
            {
                return await _service.SearchReleases(query, regions, platforms);
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        [HttpGet("ical")]
        public async Task<FileContentResult> GetReleaseCalendarAsync([FromQuery(Name = "regions")] int[] regionFilter, [FromQuery(Name = "platforms")] int[] platformFilter)
        {
            try
            {
                return await _service.GetCalendar(regionFilter, platformFilter);
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }
    }
}