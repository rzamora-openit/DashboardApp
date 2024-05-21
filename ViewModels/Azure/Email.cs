using System.Collections.Generic;

namespace OpeniT.PowerbiDashboardApp.ViewModels.Azure
{
	public class Email
	{
		public string Subject { get; set; }

		public EmailBody Body { get; set; }

		public IEnumerable<EmailRecipients> ToRecipients { get; set; }

		public IEnumerable<EmailRecipients> CcRecipients { get; set; }

		public IEnumerable<EmailRecipients> BccRecipients { get; set; }
	}
}
