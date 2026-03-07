namespace Project2EmailNight.Models
{
	public interface IEmailService
	{
		Task SendAsync(string toEmail, string subject, string body);
	}
}
