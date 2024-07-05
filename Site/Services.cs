using OpeniT.PowerbiDashboardApp.Areas.Dashboard.Controllers;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Security.Model;
using OpeniT.PowerbiDashboardApp.ViewModels.NotInModels;
using System.Collections.Generic;
using System.Linq;

namespace OpeniT.PowerbiDashboardApp.Site
{
	public static class Services
	{
		public static class FeatureNames
		{
			#region Groups
			public const string GroupDashboard = "Dashboard";
			public const string GroupAdministration = "Administration";
			#endregion Groups

			#region Services
			public const string Dashboard = "Dashboard";

			public const string Roles = "Roles";
			public const string FeatureAccess = "Access";
			#endregion Services
		}

		public const string ServiceGroupDashboardDescription = "";
		public const string ServiceGroupAdministrationDescription = "";

		public const string ServiceDashboardDescription = "";

		public const string ServiceRolesDescription = "Create and assign roles to internal users";
		public const string ServiceFeatureAccessDescription = "Control accessibility of pages based on users, roles and distribution groups";

		public static ServiceGroupViewModel ServiceGroupDashboard { get => new ServiceGroupViewModel() { Id = 1, Index = 1, Name = FeatureNames.Dashboard, Description = ServiceGroupDashboardDescription, Icon = "fa-chart-pie", Uri = "" }; }
		public static ServiceGroupViewModel ServiceGroupAdministration { get => new ServiceGroupViewModel() { Id = 2, Index = 2, Name = FeatureNames.GroupAdministration, Description = ServiceGroupAdministrationDescription, Icon = "fa-user-shield", Uri = "" }; }

		public static ServiceViewModel ServiceDashboard { get => new ServiceViewModel() { Id = 1, Index = 1, Name = FeatureNames.Dashboard, Description = ServiceRolesDescription, Icon = "fa-chart-pie", Uri = "/dashboard", Controller = nameof(DashboardController) }; }

		public static ServiceViewModel ServiceRoles { get => new ServiceViewModel() { Id = 2, Index = 2, Name = FeatureNames.Roles, Description = ServiceRolesDescription, Icon = "fa-user-lock", Uri = "/dashboard/roles", Controller = nameof(RolesController) }; }
		public static ServiceViewModel ServiceFeatureAccess { get => new ServiceViewModel() { Id = 3, Index = 3, Name = FeatureNames.FeatureAccess, Description = ServiceFeatureAccessDescription, Icon = "fa-door-open", Uri = "/dashboard/featureaccess", Controller = nameof(FeatureAccessController) }; }

		public static readonly IEnumerable<string> ServiceGroupFeatureList = new List<string>()
		{
			FeatureNames.GroupAdministration
		};

		public static readonly IEnumerable<string> ExemptServiceFeatureList = new List<string>()
		{

		};

		public static readonly IEnumerable<string> ServiceFeatureList = new List<string>()
		{
			FeatureNames.Dashboard,
			FeatureNames.Roles,
			FeatureNames.FeatureAccess
		};

		public static readonly Dictionary<string, string> FeatureToServiceGroupParentMap = new Dictionary<string, string>()
		{
			{FeatureNames.Dashboard,                    FeatureNames.GroupDashboard},
			{FeatureNames.Roles,                        FeatureNames.GroupAdministration},
			{FeatureNames.FeatureAccess,                FeatureNames.GroupAdministration}
		};

		public static ServiceGroupViewModel GenerateServiceGroup(string feature)
		{
			switch (feature)
			{
				case FeatureNames.GroupAdministration: return ServiceGroupAdministration;
				default: return new ServiceGroupViewModel() { };
			};
		}

		public static ServiceViewModel GenerateService(string feature)
		{
			switch (feature)
			{
				case FeatureNames.Dashboard: return ServiceDashboard;
				case FeatureNames.Roles: return ServiceRoles;
				case FeatureNames.FeatureAccess: return ServiceFeatureAccess;

				default: return new ServiceViewModel() { };
			};
		}

		public static List<ServiceGroupViewModel> GenerateServiceGroups(AccessProfile accessProfile, Security.AccessLevelFlag levelFlag = Security.AccessLevelFlag.Read)
		{
			SortedList<string, ServiceGroupViewModel> serviceGroupViewModels = new SortedList<string, ServiceGroupViewModel>();

			//Adds the Excempt Feature
			foreach (var serviceName in Services.ExemptServiceFeatureList)
			{
				var parentServiceGroup = Services.FeatureToServiceGroupParentMap[serviceName];
				//Ensure add the Service Group
				if (!serviceGroupViewModels.ContainsKey(parentServiceGroup))
				{
					var serviceGroupViewModel = Services.GenerateServiceGroup(parentServiceGroup);
					//Reset Services List
					serviceGroupViewModel.Services = new List<ServiceViewModel>();
					serviceGroupViewModels.Add(parentServiceGroup, serviceGroupViewModel);
				}
				//Add the service
				var serviceModel = Services.GenerateService(serviceName);
				serviceGroupViewModels[parentServiceGroup].Services.Add(serviceModel);
			}

			//Evaluate each service listed in meta information
			foreach (var serviceName in Services.ServiceFeatureList)
			{
				if (Site.StaticValues.EnableGlobalInternalAccess ||
					Security.AccessEvaluator.AssertAccessLevel(FeatureAccessHelper.GetFeatureAccess(serviceName), accessProfile) >= levelFlag)
				{
					var parentServiceGroup = Services.FeatureToServiceGroupParentMap[serviceName];
					//Ensure add the Service Group
					if (!serviceGroupViewModels.ContainsKey(parentServiceGroup))
					{
						var serviceGroupViewModel = Services.GenerateServiceGroup(parentServiceGroup);
						//Reset Services List
						serviceGroupViewModel.Services = new List<ServiceViewModel>();
						serviceGroupViewModels.Add(parentServiceGroup, serviceGroupViewModel);
					}
					//Add the service
					var serviceModel = Services.GenerateService(serviceName);
					serviceGroupViewModels[parentServiceGroup].Services.Add(serviceModel);
				}
			}

			return serviceGroupViewModels.Values.ToList();
		}

		public static List<ServiceViewModel> GenerateServices(AccessProfile accessProfile, Security.AccessLevelFlag levelFlag = Security.AccessLevelFlag.Read)
		{
			if (Site.StaticValues.EnableGlobalInternalAccess)
			{
				return ServiceFeatureList.Select(x => GenerateService(x)).OrderBy(x => x.Index).ToList();
			}
			else
			{
				List<ServiceViewModel> services = new List<ServiceViewModel>();


				//Evaluate each service listed in meta information
				foreach (var serviceName in Services.ServiceFeatureList)
				{
					var featureAccesses = Helpers.FeatureAccessHelper.FeatureAccesses.ContainsKey(serviceName) ?
											Helpers.FeatureAccessHelper.FeatureAccesses[serviceName] :
											new List<Models.Application.Access>();
					var accessLevel = Security.AccessEvaluator.AssertAccessLevel(featureAccesses, accessProfile);

					if (accessLevel >= levelFlag)
					{
						//Add the service
						var serviceModel = Services.GenerateService(serviceName);
						services.Add(serviceModel);
					}
				}

				return services;
			}
		}
	}
}
