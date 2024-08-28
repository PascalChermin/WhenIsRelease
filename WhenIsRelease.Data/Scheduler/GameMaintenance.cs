using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using WhenIsRelease.Data.Models.Context;
using WhenIsRelease.Models.GameReleases;

namespace WhenIsRelease.Data.Scheduler
{
    internal enum ApiTargets
    {
        Regions,
        Platforms,
        Releases
    }

    public class GameMaintenance : IMaintenance
    {
        private const string sourceUrl = "https://www.giantbomb.com/api/{0}/?api_key={1}";
        private readonly ILogger _logger;
        private readonly ISocial _twitter;
        private readonly string _companyName;
        private readonly string _productName;
        private readonly string _productVersion;
        private bool _inProgress;

        public GameMaintenance(ILogger<GameMaintenance> logger, ISocial twitter)
        {
            _logger = logger;
            _twitter = twitter;
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            _companyName = fvi.CompanyName;
            _productName = fvi.ProductName;
            _productVersion = fvi.ProductVersion;
        }

        public async Task UpdateSingleItem(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(string.Format(sourceUrl, $"release/{id}", Environment.GetEnvironmentVariable("GBAPIkey")));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_productName, _productVersion));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(client.BaseAddress);

                _logger.LogDebug($"Got response with status '{response.StatusCode}'");
                if (!response.IsSuccessStatusCode) return;

                await response.Content.ReadAsAsync<dynamic>();
            }
        }

        public async Task UpdateDatabaseAsync()
        {
            _logger.LogTrace($"Started function {nameof(UpdateDatabaseAsync)}");

            try
            {
                if (_inProgress)
                    return;

                _inProgress = true;

                await UpdateRegions();
                await UpdatePlatforms();
                await UpdateReleases();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception in {nameof(UpdateDatabaseAsync)}");
            }
            finally
            {
                _inProgress = false;
                _logger.LogTrace($"Ended function {nameof(UpdateDatabaseAsync)}");
            }
        }

        public void TweetReleasesOfTheDay()
        {
            _logger.LogTrace($"Started function {nameof(TweetReleasesOfTheDay)}");

            try
            {
                using (var db = new GameContext())
                {
                    var releases = db.GameRelease
                        .Include(g => g.Platform)
                        .Include(g => g.Region)
                        .Where(r => r.ReleaseDate.Date == DateTime.Today);
                    _twitter.PublishReleasesOfTheDay(releases);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception in {nameof(TweetReleasesOfTheDay)}");
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(TweetReleasesOfTheDay)}");
            }
        }

        private void ProcessRegions(dynamic data)
        {
            _logger.LogTrace($"Started function {nameof(ProcessRegions)}");

            try
            {
                foreach (var reg in data.results)
                {
                    using (var db = new GameContext())
                    {
                        int rid = reg.id ?? default(int);
                        var region = db.Region.FirstOrDefault(r => r.SourceId == rid);

                        if (region != null)
                        {
                            if (region.LastUpdate >= DateTime.Parse(reg.date_last_updated.ToString())) continue;

                            _logger.LogDebug($"Updating region '{region.Name}' ({region.SourceId})");
                            region.Name = reg.name.ToString().Replace(Environment.NewLine, string.Empty);
                            region.Image = reg.image.original_url.ToString();
                            region.LastUpdate = DateTime.Parse(reg.date_last_updated.ToString());
                            db.SaveChanges();
                        }
                        else
                        {
                            _logger.LogDebug($"Creating new region '{reg.name}' ({reg.id})");
                            db.Region.Add(new Region
                            {
                                SourceId = int.Parse(reg.id.ToString()),
                                Name = reg.name.ToString().Replace(Environment.NewLine, string.Empty),
                                Image = reg.image.original_url.ToString(),
                                LastUpdate = DateTime.Parse(reg.date_last_updated.ToString())
                            });
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception in {nameof(ProcessRegions)}");
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(ProcessRegions)}");
            }
        }

        private void ProcessPlatforms(dynamic data)
        {
            _logger.LogTrace($"Started function {nameof(ProcessPlatforms)}");

            try
            {
                foreach (var plt in data.results)
                {
                    using (var db = new GameContext())
                    {
                        int sid = plt.id ?? default(int);
                        var platform = db.Platform.FirstOrDefault(g => g.SourceId == sid);

                        if (platform != null)
                        {
                            if (platform.LastUpdate >= DateTime.Parse(plt.date_last_updated.ToString())) continue;

                            _logger.LogDebug($"Updating platform '{platform.Name}' ({platform.SourceId})");
                            platform.Name = plt.name.ToString().Replace(Environment.NewLine, string.Empty);
                            platform.Abbreviation = plt.abbreviation.ToString();
                            platform.Url = plt.site_detail_url.ToString();
                            platform.Image = plt.image.original_url.ToString();
                            platform.LastUpdate = DateTime.Parse(plt.date_last_updated.ToString());
                            db.SaveChanges();
                        }
                        else
                        {
                            _logger.LogDebug($"Creating new platform '{plt.name}' ({plt.id})");
                            db.Platform.Add(new Platform
                            {
                                SourceId = int.Parse(plt.id.ToString()),
                                Name = plt.name.ToString().Replace(Environment.NewLine, string.Empty),
                                Abbreviation = plt.abbreviation.ToString(),
                                Url = plt.site_detail_url.ToString(),
                                Image = plt.image.original_url.ToString(),
                                LastUpdate = DateTime.Parse(plt.date_last_updated.ToString())
                            });
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception in {nameof(ProcessPlatforms)}");
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(ProcessPlatforms)}");
            }
        }

        private void ProcessReleases(dynamic data)
        {
            _logger.LogTrace($"Started function {nameof(ProcessReleases)}");

            try
            {
                foreach (var release in data.results)
                {
                    using (var db = new GameContext())
                    {
                        int sid = release.id ?? default(int);
                        var rel = db.GameRelease.FirstOrDefault(g => g.SourceId == sid);

                        if (rel != null)
                        {
                            if (rel.LastUpdate >= DateTime.Parse(release.date_last_updated.ToString())) continue;

                            _logger.LogDebug($"Updating game '{rel.Name}' ({rel.SourceId})");
                            var platformSourceId = (int)release.platform.id;
                            var regionSourceId = (int)release.region.id;
                            rel.Name = release.name.ToString().Replace(Environment.NewLine, string.Empty);
                            rel.Url = release.site_detail_url.ToString();
                            rel.Image = release.image.original_url.ToString();
                            rel.LastUpdate = DateTime.Parse(release.date_last_updated.ToString());
                            rel.Platform = db.Platform.FirstOrDefault(p => p.SourceId == platformSourceId);
                            rel.Region = db.Region.FirstOrDefault(r => r.SourceId == regionSourceId);
                            rel.ReleaseDate = DetermineReleaseDate(release);
                            db.SaveChanges();
                        }
                        else
                        {
                            _logger.LogDebug($"Creating new game '{release.name}' ({release.id})");
                            var platformSourceId = (int)release.platform.id;
                            var regionSourceId = string.IsNullOrWhiteSpace(release.region.ToString()) ? 0 : (int)release.region.id;
                            db.GameRelease.Add(new Release
                            {
                                SourceId = int.Parse(release.id.ToString()),
                                Name = release.name.ToString().Replace(Environment.NewLine, string.Empty),
                                Url = release.site_detail_url.ToString(),
                                Image = release.image.original_url.ToString(),
                                LastUpdate = DateTime.Parse(release.date_last_updated.ToString()),
                                Platform = db.Platform.FirstOrDefault(p => p.SourceId == platformSourceId),
                                Region = db.Region.FirstOrDefault(r => r.SourceId == regionSourceId),
                                ReleaseDate = DetermineReleaseDate(release)
                            });
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception in {nameof(ProcessReleases)}");
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(ProcessReleases)}");
            }
        }

        private async Task UpdateRegions()
        {
            _logger.LogTrace($"Started function {nameof(UpdateRegions)}");

            try
            {
                DateTime? lastRegionUpdate;

                using (var db = new GameContext())
                {
                    lastRegionUpdate = db.Region.Max(p => (DateTime?)p.LastUpdate);
                }

                await UpdateData(ApiTargets.Regions, lastRegionUpdate, ProcessRegions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(UpdateRegions)}");
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(UpdateRegions)}");
            }
        }

        private async Task UpdatePlatforms()
        {
            _logger.LogTrace($"Started function {nameof(UpdatePlatforms)}");

            try
            {
                DateTime? lastPlatformUpdate;

                using (var db = new GameContext())
                {
                    lastPlatformUpdate = db.Platform.Max(p => (DateTime?)p.LastUpdate);
                }

                await UpdateData(ApiTargets.Platforms, lastPlatformUpdate, ProcessPlatforms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(UpdatePlatforms)}");
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(UpdatePlatforms)}");
            }
        }

        private async Task UpdateReleases()
        {
            _logger.LogTrace($"Started function {nameof(UpdateReleases)}");

            try
            {
                DateTime? lastGameUpdate;

                using (var db = new GameContext())
                {
                    lastGameUpdate = db.GameRelease.Max(p => (DateTime?)p.LastUpdate);
                }

                await UpdateData(ApiTargets.Releases, lastGameUpdate, ProcessReleases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(UpdateReleases)}");
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(UpdateReleases)}");
            }
        }

        private async Task UpdateData(ApiTargets target, DateTime? lastUpdate, Action<dynamic> processMethod)
        {
            _logger.LogTrace($"Started function {nameof(UpdateData)}");

            try
            {
                var data = await GetData(target, lastUpdate: lastUpdate);
                processMethod(data);
                var newOffset = int.Parse(data.offset.ToString()) + int.Parse(data.number_of_page_results.ToString());
                var totalAmount = int.Parse(data.number_of_total_results.ToString());
                while (newOffset < totalAmount)
                {
                    data = await GetData(target, offset: newOffset, lastUpdate: lastUpdate);
                    processMethod(data);
                    newOffset = int.Parse(data.offset.ToString()) + int.Parse(data.number_of_page_results.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {nameof(UpdateData)}");
            }
            finally
            {
                _logger.LogTrace($"Ended function {nameof(UpdateData)}");
            }
        }

        private async Task<dynamic> GetData(ApiTargets target, bool noFilter = false, int offset = 0, DateTime? lastUpdate = null)
        {
            // https://stackoverflow.com/questions/22912021/exception-at-initialization-of-datetime
            // Need to use nullable DateTime and set to min when entering GetData if it is :(
            if (lastUpdate == null) lastUpdate = DateTime.MinValue;

            using (var client = new HttpClient())
            {
                var filter = noFilter ? string.Empty : $"&filter=date_last_updated:{lastUpdate:yyyy-MM-dd}%2000:00:00|{DateTime.Today:yyyy-MM-dd}%2023:59:59";
                var order = noFilter ? "&order=id" : "&order=date_last_updated";
                client.BaseAddress = new Uri(string.Format(sourceUrl, target.ToString().ToLower(), Environment.GetEnvironmentVariable("GBAPIkey")));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_productName, _productVersion));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync($"{client.BaseAddress}&format=json&offset={offset}{filter}{order}");

                _logger.LogDebug($"Got response with status '{response.StatusCode}'");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadAsAsync<dynamic>();
            }
        }

        private static DateTime DetermineReleaseDate(dynamic release)
        {
            if (!string.IsNullOrWhiteSpace(release.release_date.ToString()))
                return DateTime.Parse(release.release_date.ToString());

            var year = string.IsNullOrWhiteSpace(release.expected_release_year.ToString())
                ? DateTime.MaxValue.Year
                : int.Parse(release.expected_release_year.ToString());
            var month = string.IsNullOrWhiteSpace(release.expected_release_month.ToString())
                ? DateTime.MaxValue.Month
                : int.Parse(release.expected_release_month.ToString());
            var day = string.IsNullOrWhiteSpace(release.expected_release_day.ToString())
                ? DateTime.MaxValue.Day
                : int.Parse(release.expected_release_day.ToString());

            return new DateTime(year, month, day);
        }
    }
}
