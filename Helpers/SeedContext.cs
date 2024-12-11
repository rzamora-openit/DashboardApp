using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;

namespace OpeniT.PowerbiDashboardApp.Helpers
{
    public class SeedContext : ISeedContext
	{
		private readonly IDataRepository dataRepository;

		public SeedContext(IDataRepository dataRepository)
		{
			this.dataRepository = dataRepository;
		}

		public async Task<bool> Seed()
		{
			try
			{
				bool ret = true;
				ret &= await SeedFeatureAccess();
				return ret;
			}
			catch (Exception ex)
			{
				return false;
			}

		}

		public async Task<bool> SeedFeatureAccess()
		{
			try
			{
				bool ret = true;
				var vars = await dataRepository.GetFeatureAccessesAsync();

				foreach (var feature in Site.Services.ServiceFeatureList)
				{
					if (!vars.Where(x => x.FeatureName == feature).Any())
					{
						dataRepository.AddFeatureAccess(new Models.Application.FeatureAccess() { FeatureName = feature }); ret &= await this.dataRepository.SaveChangesAsync();
					}
				}

				return ret;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
	}
}
