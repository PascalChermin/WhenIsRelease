using System;

namespace WhenIsRelease.Models
{
    public interface IRelease
    {
        int Id { get; set; }
        int SourceId { get; set; }
        string Name { get; set; }
        DateTime ReleaseDate { get; set; }
        DateTime LastUpdate { get; set; }
    }
}
