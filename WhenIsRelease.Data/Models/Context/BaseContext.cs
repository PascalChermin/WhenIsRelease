using Microsoft.EntityFrameworkCore;
using System;

namespace WhenIsRelease.Data.Models.Context
{
    public class BaseContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            optionsBuilder.UseSqlServer("Server=localhost,1435;Database=WhenIsRelease;User Id=sa;Password=yourStrong(!)Password");
#else
            optionsBuilder.UseSqlite($"Data Source={Environment.GetEnvironmentVariable("WhenIsReleaseDataDB")};");
#endif
        }
    }
}
