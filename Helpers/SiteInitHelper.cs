using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using System.Threading.Tasks;
using System;

namespace OpeniT.PowerbiDashboardApp.Helpers
{
    public class SiteInitHelper : ISiteInitHelper
	{
		private readonly IDataRepository dataRepository;
		private readonly IFeatureAccessHelper featureAccessHelper;

		public SiteInitHelper(IDataRepository dataRepository,
			IFeatureAccessHelper featureAccessHelper)
		{
			this.dataRepository = dataRepository;
			this.featureAccessHelper = featureAccessHelper;
		}

		public async Task<bool> Init()
		{
			try
			{
				bool ret = true;

				ret &= await InitFeatureAccess();

				return ret;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public async Task<bool> InitFeatureAccess()
		{
			try
			{
				var featureAccesses = await dataRepository.GetFeatureAccessesAsync();
				foreach (var featureAccess in featureAccesses)
				{
					await featureAccessHelper.AddFeatureAccess(featureAccess.FeatureName);

					foreach (var access in featureAccess.Accesses)
					{
						await featureAccessHelper.AddFeatureAccess(featureAccess.FeatureName, access);
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}
