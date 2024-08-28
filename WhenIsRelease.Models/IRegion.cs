using System;

namespace WhenIsRelease.Models
{
    public interface IRegion
    {
        int Id { get; set; }
        int SourceId { get; set; }
        string Name { get; set; }
        DateTime LastUpdate { get; set; }
    }
}
