namespace OpeniT.PowerbiDashboardApp.ViewModels.Azure
{
	public class Error
	{
		public string Code { get; set; }
		public string Message { get; set; }
		public Error InnerError { get; set; }
	}
}
