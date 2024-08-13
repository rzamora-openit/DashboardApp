using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using OpeniT.PowerbiDashboardApp.Data;
using OpeniT.PowerbiDashboardApp.Data.Interfaces;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Helpers.Interfaces;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Security;
using OpeniT.PowerbiDashboardApp.Security.Handler;
using System.Reflection;

namespace OpeniT.PowerbiDashboardApp
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			var services = builder.Services;

			var migrationsAssembly = typeof(DataContext).GetTypeInfo().Assembly.GetName().Name;

			services.AddDbContext<DataContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DataConnection"),
				b => b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery).MigrationsAssembly(migrationsAssembly)),
				ServiceLifetime.Transient);

			services.AddTransient<IApplicationLogger, ApplicationLogger>();
			services.AddTransient<IDataRepository, DataRepository>();

			services.AddSingleton<IAzureQueryHelper, AzureQueryHelper>();
			services.AddSingleton<IAzureQueryRepository, AzureQueryRepository>();

			services.AddSingleton<IPowerBIQueryHelper, PowerBIQueryHelper>();
			services.AddSingleton<IPowerBIQueryRepository, PowerBIQueryRepository>();
			services.AddScoped<IPowerBIEmbedHelper, PowerBIEmbedHelper>();

			services.AddScoped<IAccessProfileHelper, AccessProfileHelper>();
			services.AddScoped<IFeatureAccessHelper, FeatureAccessHelper>();
			services.AddSingleton<AzureStaticStore>();

			services.AddTransient<ISeedContext, SeedContext>();
			services.AddTransient<ISiteInitHelper, SiteInitHelper>();

			services.AddTransient<IAuthorizationHandler, FeatureAccessHandler>();
			services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();

			services.AddAuthentication()
			   .AddOpenIdConnect(options =>
			   {
				   options.Authority = builder.Configuration["Microsoft:Authority"];
				   options.ClientId = builder.Configuration["Microsoft:ClientId"];
				   options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
			   });

			services.AddAuthorizationBuilder()
				.AddPolicy("Internal", policy => policy.RequireAssertion(Security.Assertions.IsInternal));

			services.AddIdentity<ApplicationUser, IdentityRole>()
			   .AddEntityFrameworkStores<DataContext>()
			   .AddDefaultTokenProviders();

			services.ConfigureApplicationCookie(options =>
			{
				options.AccessDeniedPath = new PathString("/NotFound");
			});

			services.Configure<CookiePolicyOptions>(options =>
			{
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;

				options.HttpOnly = HttpOnlyPolicy.Always;
				options.Secure = CookieSecurePolicy.Always;
			});

			services
			  .AddControllersWithViews(options =>
			  {
				  var policy = new AuthorizationPolicyBuilder()
					  .RequireAuthenticatedUser()
					  .Build();
				  options.Filters.Add(new AuthorizeFilter(policy));
			  })
			  .AddNewtonsoftJson(options =>
			  {
				  options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			  });

			services.AddHttpContextAccessor();

			var app = builder.Build();

			if (!app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			app.UseStaticFiles();
			app.UseRouting();
			app.UseCookiePolicy();

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllerRoute(name: "areaRoute", pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
			app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

			#region preprocess
			using (var serviceScope = app.Services.GetService<IServiceScopeFactory>().CreateScope())
			{
				var seedContext = serviceScope.ServiceProvider.GetService<ISeedContext>();
				var siteInithelper = serviceScope.ServiceProvider.GetService<ISiteInitHelper>();

				seedContext.Seed().Wait();
				siteInithelper.InitFeatureAccess().Wait();
			}
			#endregion preprocess

			app.Run();
		}
	}
}
