using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpeniT.PowerbiDashboardApp.Models.Accounts;
using OpeniT.PowerbiDashboardApp.Models.Application;
using OpeniT.PowerbiDashboardApp.Models.Files;
using OpeniT.PowerbiDashboardApp.Models.Objects;

namespace OpeniT.PowerbiDashboardApp.Data
{
	public class DataContext : IdentityDbContext<ApplicationUser>
	{
		public DataContext(DbContextOptions<DataContext> options)
		   : base(options)
		{
		}

		public DbSet<ApplicationActivity> Activities { get; set; }

		public DbSet<ApplicationUser> ApplicationUsers { get; set; }
		public DbSet<InternalAccount> InternalAccounts { get; set; }

		public DbSet<Image> Images { get; set; }
		public DbSet<Blob> Blobs { get; set; }

		public DbSet<PowerbiReference> PowerbiReferences { get; set; }

		public DbSet<FeatureAccess> FeatureAccesses { get; set; }
		public DbSet<Access> Accesses { get; set; }

		public DbSet<Sharing> Sharings { get; set; }
		public DbSet<UserShare> UserShares { get; set; }
		public DbSet<GroupShare> GroupShares { get; set; }
	}
}
