using System.Threading.Tasks;

namespace WhenIsRelease.Data.Scheduler
{
    public interface IMaintenance
    {
        Task UpdateSingleItem(int id);
        Task UpdateDatabaseAsync();
    }
}