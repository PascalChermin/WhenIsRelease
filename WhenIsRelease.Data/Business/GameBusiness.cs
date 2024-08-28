using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using WhenIsRelease.Data.Business;
using WhenIsRelease.Models;
using WhenIsRelease.Models.GameReleases;
using WhenIsRelease.Data.Models.Context;
using WhenIsRelease.Data.Scheduler;

namespace WhenIsRelease.Data
{
    public class GameBusiness : IGameBusiness
    {
        private readonly GameContext _context;
        private readonly IMaintenance _maintenance;
        private readonly CalendarBuilder _calendarBuilder;

        public GameBusiness(GameContext context, IMaintenance maintenance)
        {
            _context = context;
            _maintenance = maintenance;
            _calendarBuilder = new CalendarBuilder();
        }

        public IRelease GetRelease(int id)
        {
            return _context.GameRelease
                .Include(g => g.Platform)
                .Include(g => g.Region)
                .FirstOrDefault(g => g.Id == id);
        }

        public List<Platform> GetAllPlatforms()
        {
            return _context.Platform.ToList();
        }

        public List<Platform> GetPlatforms()
        {
            return _context.GameRelease
                .Include(g => g.Platform)
                .Where(g => g.ReleaseDate > DateTime.Now.AddMonths(-6) && g.ReleaseDate < DateTime.MaxValue.Date)
                .Select(g => g.Platform)
                .Distinct().ToList();
        }

        public List<Region> GetRegions()
        {
            return _context.Region.ToList();
        }

        public string GetReleaseCalendar(List<int[]> filters)
        {
            var results = _context.GameRelease
                .Include(g => g.Platform)
                .Include(g => g.Region);

            if (filters[0].Length > 0)
                results = results
                    .Where(g => filters[0].Contains(g.Region.Id) || g.Region == null)
                    .Include(g => g.Platform)
                    .Include(g => g.Region);

            if (filters[1].Length > 0)
                results = results
                    .Where(g => filters[1].Contains(g.Platform.Id))
                    .Include(g => g.Platform)
                    .Include(g => g.Region);
            
            results = results
                .Where(g => g.ReleaseDate >= DateTime.Now.Date && g.ReleaseDate < DateTime.Now.Date.AddYears(5))
                .Include(g => g.Platform)
                .Include(g => g.Region);

            return _calendarBuilder.Build(results);
        }

        public List<IRelease> GetReleases(DateTime startDate, DateTime endDate)
        {
            return _context.GameRelease
                .Include(g => g.Platform)
                .Include(g => g.Region)
                .Where(g => g.ReleaseDate >= startDate && g.ReleaseDate < endDate)
                .ToList<IRelease>();
        }

        public List<IRelease> Search(string query)
        {
            return _context.GameRelease
                .Include(g => g.Platform)
                .Include(g => g.Region)
                .Where(g => g.Name.ToLower().Contains(query.ToLower()))
                .Take(50)
                .ToList<IRelease>();
        }

        public void UpdateDatabase()
        {
            _ = _maintenance.UpdateDatabaseAsync();
        }

        public List<IRelease> Search(string query, int[] regions, int[] platforms)
        {
            return _context.GameRelease
                .Include(g => g.Platform)
                .Include(g => g.Region)
                .Where(g => g.Name.ToLower().Contains(query.ToLower()))
                .Where(g => g.Region == null || regions.Count() == 0 || regions.Contains(g.Region.Id))
                .Where(g => g.Platform == null || platforms.Count() == 0 || platforms.Contains(g.Platform.Id))
                .Take(50)
                .ToList<IRelease>();
        }
    }
}
