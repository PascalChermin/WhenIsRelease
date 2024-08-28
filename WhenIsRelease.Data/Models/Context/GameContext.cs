using Microsoft.EntityFrameworkCore;
using WhenIsRelease.Models.GameReleases;

namespace WhenIsRelease.Data.Models.Context
{
    public class GameContext : BaseContext
    {
        public DbSet<Release> GameRelease { get; set; }
        public DbSet<Platform> Platform { get; set; }
        public DbSet<Region> Region { get; set; }

        public GameContext() : base() { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("game");
        }
    }
}
