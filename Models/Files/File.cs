using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpeniT.PowerbiDashboardApp.Models.Files
{
	public class File
	{
		public int Id { get; set; }

		[NotMapped]
		public string FileName_ { get; set; }
		public string FileName
		{
			get { return Methods.Utility.SanitizeFileName(FileName_); }
			set { FileName_ = Methods.Utility.SanitizeFileName(value); }
		}

		public string FileTypeInfo { get; set; }

		public DateTime UploadedDate { get; set; }

		public string UploadedByEmail { get; set; }

		public string FileUrl { get; set; }

		public int? BlobId { get; set; }
		public Blob Blob { get; set; }
	}
}
