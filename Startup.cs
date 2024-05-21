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
using Microsoft.Identity.Web.UI;
using Newtonsoft.Json;
using OpeniT.PowerbiDashboardApp.Data;
using OpeniT.PowerbiDashboardApp.Helpers;
using OpeniT.PowerbiDashboardApp.Models.Accounts;

namespace OpeniT.PowerbiDashboardApp
{
	public class Startup
	{
		public Startup(IWebHostEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", false, true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
				.AddEnvironmentVariables();
			this.Configuration = builder.Build();
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(this.Configuration);

			services.AddDbContext<DataContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DataConnection")), ServiceLifetime.Transient);

			services.AddTransient<ApplicationLogger>();
			services.AddTransient<DataRepository>();

			services.AddSingleton<AzureQueryHelper>();
			services.AddSingleton<AzureQueryRepository>();

			services.AddSingleton<PowerBIQueryHelper>();
			services.AddSingleton<PowerBIQueryRepository>();
			services.AddScoped<PowerBIEmbedHelper>();

			services.AddAuthentication()
			   .AddOpenIdConnect(options =>
			   {
				   options.Authority = this.Configuration["Microsoft:Authority"];
				   options.ClientId = this.Configuration["Microsoft:ClientId"];
				   options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
			   });

			services.AddAuthorization(options =>
			{
				options.AddPolicy("Internal", policy => policy.RequireAssertion(Security.Assertions.IsInternal));
			});

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
			  //.AddMvc()
			  .AddNewtonsoftJson(options =>
			  {
				  options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
				  //options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
			  });

			services.AddHttpContextAccessor();
			services.AddRazorPages()
				 .AddMicrosoftIdentityUI();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
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

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(name: "areaRoute", pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
				endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
