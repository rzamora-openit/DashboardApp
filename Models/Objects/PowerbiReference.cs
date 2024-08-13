namespace OpeniT.PowerbiDashboardApp.Models.Objects
{
	public class PowerbiReference
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string WorkGroupId { get; set; }
		public string DataSetId { get; set; }
		public string ReportId { get; set; }
        public Sharing Sharing { get; set; }
    }
}
