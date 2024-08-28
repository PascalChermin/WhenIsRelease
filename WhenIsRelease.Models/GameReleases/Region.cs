using System;

namespace WhenIsRelease.Models.GameReleases
{
    public class Region : IRegion
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
