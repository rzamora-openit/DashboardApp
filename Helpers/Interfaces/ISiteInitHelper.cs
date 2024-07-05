using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Helpers.Interfaces
{
    public interface ISiteInitHelper
    {
        Task<bool> Init();
        Task<bool> InitFeatureAccess();
    }
}