using System.Collections.Generic;
using WhenIsRelease.Models;
using WhenIsRelease.Models.GameReleases;

namespace WhenIsRelease.Data.Business
{
    interface IGameBusiness : IBusiness
    {
        List<Platform> GetAllPlatforms();
        List<Platform> GetPlatforms();
        List<Region> GetRegions();
        void UpdateDatabase();
        List<IRelease> Search(string query, int[] regions, int[] platforms);
    }
}
