using OpeniT.PowerbiDashboardApp.Models.Application;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;

namespace OpeniT.PowerbiDashboardApp.Helpers
{
    public class FeatureAccessHelper : IFeatureAccessHelper
	{
		private SemaphoreSlim singleTask = new SemaphoreSlim(1);
		public static SortedList<string, List<Access>> FeatureAccesses = new SortedList<string, List<Access>>();

		public async Task<bool> AddFeatureAccess(string feature)
		{
			await singleTask.WaitAsync();
			try
			{
				if (!FeatureAccesses.ContainsKey(feature))
				{
					FeatureAccesses.Add(feature, new List<Access>());
				}

				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				singleTask.Release();
			}
		}

		public async Task<bool> AddFeatureAccess(string feature, Access access)
		{
			await singleTask.WaitAsync();
			try
			{
				if (!FeatureAccesses.ContainsKey(feature))
				{
					FeatureAccesses.Add(feature, new List<Access>());
				}

				var accesses = FeatureAccesses[feature];
				var access_ = accesses.FirstOrDefault(x => x.Type == access.Type && x.Reference == access.Reference);
				if (access_ != null)
				{
					accesses.Remove(access_);
				}

				accesses.Add(access);
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				singleTask.Release();
			}
		}

		public async Task<bool> RemoveFeatureAccess(string feature, Access access)
		{
			await singleTask.WaitAsync();
			try
			{
				if (!FeatureAccesses.ContainsKey(feature))
				{
					FeatureAccesses.Add(feature, new List<Access>());
					return false;
				}

				var accesses = FeatureAccesses[feature];
				if (!accesses.Any(x => x.Id == access.Id))
				{
					return false;
				}

				var access_ = accesses.FirstOrDefault(x => x.Id == access.Id);
				accesses.Remove(access_);
				return true;
			}
			catch
			{
				return false;
			}
			finally
			{
				singleTask.Release();
			}
		}

		public async Task<bool> RemoveFeatureAccess(Access access)
		{
			await singleTask.WaitAsync();
			try
			{
				IList<string> keys = FeatureAccesses.Keys;
				foreach (var key in keys)
				{
					if (FeatureAccesses[key].Any(x => x.Id == access.Id))
					{
						var access_ = FeatureAccesses[key].FirstOrDefault(x => x.Id == access.Id);
						FeatureAccesses[key].Remove(access_);
						return true;
					}
				}
				return false;
			}
			catch
			{
				return false;
			}
			finally
			{
				singleTask.Release();
			}
		}

		public async Task<List<Access>> GetFeatureAccessAsync(string feature)
		{
			await singleTask.WaitAsync();
			try
			{
				return GetFeatureAccess(feature);
			}
			catch
			{
				return new List<Access>();
			}
			finally
			{
				singleTask.Release();
			}
		}

		public static List<Access> GetFeatureAccess(string feature)
		{
			return FeatureAccesses.ContainsKey(feature) ? FeatureAccesses[feature] : new List<Access>();
		}
	}
}
