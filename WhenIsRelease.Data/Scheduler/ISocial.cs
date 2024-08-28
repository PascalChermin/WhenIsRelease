using System.Collections.Generic;
using WhenIsRelease.Models;

namespace WhenIsRelease.Data.Scheduler
{
    public interface ISocial
    {
        void PublishReleasesOfTheDay(IEnumerable<IRelease> releases);
    }
}
