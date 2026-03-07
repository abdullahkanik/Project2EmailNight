using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Project2EmailNight.Models
{
	public class EmailService : IEmailService
	{
		private readonly EmailSettings _settings;

		public EmailService(IOptions<EmailSettings> settings)
		{
			_settings = settings.Value;
		}

		public async Task SendAsync(string toEmail, string subject, string body)
		{
			using var client = new SmtpClient(_settings.Host, _settings.Port)
			{
				Credentials = new NetworkCredential(_settings.FromEmail, _settings.AppPassword),
				EnableSsl = _settings.EnableSsl
			};

			var mailMessage = new MailMessage
			{
				From = new MailAddress(_settings.FromEmail),
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};

			mailMessage.To.Add(toEmail);

			await client.SendMailAsync(mailMessage);
		}
	}
}
