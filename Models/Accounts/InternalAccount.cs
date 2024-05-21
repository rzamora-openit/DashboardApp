using System;

namespace OpeniT.PowerbiDashboardApp.Models.Accounts
{
	public class InternalAccount
	{
		public int Id { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime DateUpdated { get; set; }

		public string ReferenceId { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }
		public string DisplayName { get; set; }
	}
}
