using WhenIsRelease.Models;
using Game = WhenIsRelease.Models.GameReleases;
using Movie = WhenIsRelease.Models.MovieReleases;

namespace WhenIsRelease.Data.Utils
{
    public class ReleaseHelper
    {
        public static string GetReleaseTitle(IRelease item)
        {
            switch (item)
            {
                case Game.Release game:
                    var result = string.Empty;

                    // Construct format "[PLATFORM] NAME (REGIONFLAG)" e.g. "[N64] The Legend of Zelda: Ocarina of Time 🇪🇺"
                    result += game.Platform == null ? string.Empty : $"[{game.Platform.Abbreviation}] ";
                    result += item.Name;
                    result += game.Region == null ? string.Empty : $" {GetFlag(game.Region)}";

                    return result;
                default:
                    return item.Name;
            }
        }

        public static string GetFlag(IRegion region)
        {
            switch (region)
            {
                case Game.Region gameRegion:
                    switch (gameRegion.Id)
                    {
                        case 1:
                            return "🇺🇸";
                        case 2:
                            return "🇪🇺";
                        case 3:
                            return "🇯🇵";
                        case 4:
                            return "🇦🇺";
                        default:
                            return string.Empty;
                    }
                case Movie.Region movieRegion:
                    switch (movieRegion.Id)
                    {
                        case 1:
                            return "🇺🇸";
                        case 2:
                            return "🇪🇺";
                        case 3:
                            return "🇯🇵";
                        case 4:
                            return "🇦🇺";
                        default:
                            return string.Empty;
                    }
                default:
                    return string.Empty;
            }
        }

        public static string GetReleaseDescription(IRelease item)
        {
            switch (item)
            {
                case Game.Release game:
                    return $"<img src={game.Image} alt='{game.Name}' />";
                default:
                    return string.Empty;
            }
        }
    }
}
