using OpeniT.PowerbiDashboardApp.Models.Application;
using System;
using System.Threading.Tasks;

namespace OpeniT.PowerbiDashboardApp.Helpers.Interfaces
{
    public interface IApplicationLogger
    {
        string GetUserAgent();
        Task<string> Log(ApplicationActivity activity);
        Task<string> Log(string log);
        Task<string> LogDataAccess(ApplicationActivity activity, string relevantObject = "", string reference = "", string log = "", bool success = true);
        Task<string> LogError(ApplicationActivity activity, string relevantObject = "", string reference = "", string log = "");
        Task<string> LogException(ApplicationActivity activity, Exception exception, string log = null);
        Task<string> LogFailure(ApplicationActivity activity, string relevantObject = "", string reference = "", string log = "");
        Task<string> LogInvalidAccess(ApplicationActivity activity, string relevantObject = "", string reference = "", string log = "");
        Task<string> LogInvalidData(ApplicationActivity activity, string log);
        Task<string> LogJsonSerialized<T>(ApplicationActivity activity, T referenceObject, string relevantObject = "", string log = "", bool success = true) where T : class;
        Task<string> LogNavigation(ApplicationActivity activity, string log = "");
        Task<string> LogServerActivity(ApplicationActivity activity, string log = "", bool success = true);
        Task<string> LogUnknown(ApplicationActivity activity);
        ApplicationActivity PopulateActivityInfo(ApplicationActivity activity);
    }
}