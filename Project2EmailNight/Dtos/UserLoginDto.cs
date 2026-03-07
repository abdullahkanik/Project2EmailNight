using System.ComponentModel.DataAnnotations;

namespace Project2EmailNight.Dtos
{
	public class UserLoginDto
	{
		[Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
		public string Username { get; set; }

		[Required(ErrorMessage = "Şifre zorunludur.")]
		public string Password { get; set; }
	}
}
