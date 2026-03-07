namespace Project2EmailNight.Models
{
	public class EmailSettings
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public bool EnableSsl { get; set; }
		public string FromEmail { get; set; }
		public string AppPassword { get; set; }
	}
}
