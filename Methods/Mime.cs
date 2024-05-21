using Microsoft.AspNetCore.StaticFiles;

namespace OpeniT.PowerbiDashboardApp.Methods
{
	public static class Mime
	{

		public static string GetMimeType(string fileName)
		{
			var extensionProvider = new FileExtensionContentTypeProvider();

			if (!extensionProvider.TryGetContentType(fileName, out var contentType))
			{
				contentType = "application/octet-stream";
			}

			return contentType;
		}
	}
}
