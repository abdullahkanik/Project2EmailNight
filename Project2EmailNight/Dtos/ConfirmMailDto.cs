using System.ComponentModel.DataAnnotations;

namespace Project2EmailNight.Dtos
{
	public class ConfirmMailDto
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required(ErrorMessage = "Onay kodu zorunludur.")]
		public string ConfirmCode { get; set; }
	}
}
