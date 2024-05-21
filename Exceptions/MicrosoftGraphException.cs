using OpeniT.PowerbiDashboardApp.ViewModels.Azure;
using System;

namespace OpeniT.PowerbiDashboardApp.Exceptions
{
	public class MicrosoftGraphException : Exception
	{
		public Error Error { get; set; }
		public MicrosoftGraphException(string message) : base(message)
		{

		}
	}
}
