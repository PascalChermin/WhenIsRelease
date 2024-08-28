using System;

namespace WhenIsRelease.Models.GameReleases
{
    public class Release : IRelease
    {
        public int Id { get; set; }
        public int SourceId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public Platform Platform { get; set; }
        public DateTime ReleaseDate { get; set; }
        public Region Region { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
