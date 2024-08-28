using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WhenIsRelease.Models;
using WhenIsRelease.Models.GameReleases;

namespace WhenIsRelease.Frontend.Services
{
    public class GameService
    {
        private readonly HttpClient _client;

        public GameService(IConfiguration config, HttpClient client)
        {
            client.BaseAddress = new Uri($"{ config.GetValue<string>("DataSource") }api/Game/");
            // Accept JSON responses
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            _client = client;
        }

        public async Task<IEnumerable<Region>> GetRegions()
        {
            var response = await _client.GetAsync("regions");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<IEnumerable<Region>>();
                return result;
            }
            else
            {
                return null;
            }
        }

        public async Task<IEnumerable<Platform>> GetAllPlatforms()
        {
            var response = await _client.GetAsync("allplatforms");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<IEnumerable<Platform>>();
            }
            else
            {
                return null;
            }
        }

        public async Task<IEnumerable<Platform>> GetPlatforms()
        {
            var response = await _client.GetAsync("platforms");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<IEnumerable<Platform>>();
            }
            else
            {
                return null;
            }
        }

        public async Task<IEnumerable<IRelease>> SearchReleases(string query, int[] regions, int[] platforms)
        {
            var filter = string.Empty;
            if (regions.Any())
            {
                filter += "?regions=" + string.Join("&regions=", regions);
            }

            if (platforms.Any())
            {
                filter += filter.IndexOf('?') > -1 ? '&' : '?';
                filter += "platforms=" + string.Join("&platforms=", platforms);
            }

            var response = await _client.GetAsync($"search/{query}{filter}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<IEnumerable<Release>>();
            }
            else
            {
                return null;
            }
        }

        public async Task<FileContentResult> GetCalendar(int[] regionFilter, int[] platformFilter)
        {
            var filter = string.Empty;
            if (regionFilter.Any())
            {
                filter += "?regions=" + string.Join("&regions=", regionFilter);
            }

            if (platformFilter.Any())
            {
                filter += filter.IndexOf('?') > -1 ? '&' : '?';
                filter += "platforms=" + string.Join("&platforms=", platformFilter);
            }

            var file = await _client.GetAsync($"ical{filter}");
            return new FileContentResult(await file.Content.ReadAsByteArrayAsync(), file.Content.Headers.ContentType.MediaType)
            {
                FileDownloadName = DateTime.Now.ToShortDateString()
            };
        }
    }
}
