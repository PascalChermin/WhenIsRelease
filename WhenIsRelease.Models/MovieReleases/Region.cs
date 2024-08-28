using System;

namespace WhenIsRelease.Models.MovieReleases
{
    public class Region : IRegion
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
