using System;
using System.Collections.Generic;
using WhenIsRelease.Models;

namespace WhenIsRelease.Data
{
    public interface IBusiness
    {
        string GetReleaseCalendar(List<int[]> filters);
        IRelease GetRelease(int id);
        List<IRelease> GetReleases(DateTime startDate, DateTime endDate);
        List<IRelease> Search(string query);
    }
}
