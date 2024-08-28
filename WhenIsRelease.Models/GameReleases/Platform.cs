using System;

namespace WhenIsRelease.Models.GameReleases
{
    public class Platform
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
