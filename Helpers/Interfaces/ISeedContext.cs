using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Helpers.Interfaces
{
    public interface ISeedContext
    {
        Task<bool> Seed();
        Task<bool> SeedFeatureAccess();
    }
}