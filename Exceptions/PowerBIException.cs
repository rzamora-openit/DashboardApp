using OpeniT.PowerbiDashboardApp.ViewModels.PowerBI;
using System;

namespace OpeniT.PowerbiDashboardApp.Exceptions
{
	public class PowerBIException : Exception
	{
		public ErrorResponse Error { get; set; }
		public PowerBIException(string message) : base(message)
		{

		}
	}
}
