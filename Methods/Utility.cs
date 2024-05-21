namespace OpeniT.PowerbiDashboardApp.Methods
{
	public static class Utility
	{
		public static string SanitizeFileName(string filename)
		{
			if (filename == null) return filename;

			return filename
						.Replace("\\", "")
						.Replace("/", "")
						.Replace(":", "")
						.Replace("*", "")
						.Replace("?", "")
						.Replace("\"", "")
						.Replace("<", "")
						.Replace(">", "")
						.Replace("|", "");
		}
	}
}
