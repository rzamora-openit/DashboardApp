using OpeniT.PowerbiDashboardApp.Models.Application;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Helpers.Interfaces
{
    public interface IFeatureAccessHelper
    {
        Task<bool> AddFeatureAccess(string feature);
        Task<bool> AddFeatureAccess(string feature, Access access);
        Task<List<Access>> GetFeatureAccessAsync(string feature);
        Task<bool> RemoveFeatureAccess(Access access);
        Task<bool> RemoveFeatureAccess(string feature, Access access);
    }
}