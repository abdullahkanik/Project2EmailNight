using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2EmailNight.Dtos;
using Project2EmailNight.Entities;

namespace Project2EmailNight.Controllers
{
	public class LoginController : Controller
	{
		private readonly SignInManager<AppUser> _signInManager;
		private readonly UserManager<AppUser> _userManager;

		public LoginController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
		{
			_signInManager = signInManager;
			_userManager = userManager;
		}

		[HttpGet]
		public IActionResult UserLogin()
		{
			return View(new UserLoginDto());
		}

		[HttpPost]
		public async Task<IActionResult> UserLogin(UserLoginDto userLoginDto)
		{
			if (!ModelState.IsValid)
				return View(userLoginDto);

			// 1) kullanıcıyı username ile bul
			var user = await _userManager.FindByNameAsync(userLoginDto.Username);
			if (user == null)
			{
				ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
				return View(userLoginDto);
			}

			// 2) EmailConfirmed kontrolü
			if (!user.EmailConfirmed)
			{
				ModelState.AddModelError("", "Giriş yapmadan önce email adresinizi doğrulamalısınız.");
				return View(userLoginDto);
			}

			// 3) şifre kontrol + giriş
			var result = await _signInManager.PasswordSignInAsync(
				userLoginDto.Username,
				userLoginDto.Password,
				isPersistent: true,   // beni hatırla
				lockoutOnFailure: false
			);

			if (result.Succeeded)
			{
				return RedirectToAction("Index", "Profile");
			}

			ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
			return View(userLoginDto);
		}
	}
}
