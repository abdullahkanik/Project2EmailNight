using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2EmailNight.Dtos;
using Project2EmailNight.Entities;
using Project2EmailNight.Models;
using System.Security.Cryptography;

namespace Project2EmailNight.Controllers
{
	public class RegisterController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly IEmailService _emailService;

		public RegisterController(UserManager<AppUser> userManager, IEmailService emailService)
		{
			_userManager = userManager;
			_emailService = emailService;
		}

		[HttpGet]
		public IActionResult CreateUser()
		{
			return View(new UserRegisterDto());
		}

		[HttpPost]
		public async Task<IActionResult> CreateUser(UserRegisterDto userRegisterDto)
		{
			// 1) DataAnnotation validasyonu
			if (!ModelState.IsValid)
				return View(userRegisterDto);

			// 2) Ek güvenlik: checkbox gerçekten seçilmiş mi?
			// (Bazı durumlarda client-side işaretli gözüküp postta false gelebiliyor)
			if (!userRegisterDto.IsTermsAccepted)
			{
				ModelState.AddModelError(nameof(userRegisterDto.IsTermsAccepted), "Kullanım şartlarını kabul etmelisiniz.");
				return View(userRegisterDto);
			}

			// 3) 6 haneli kod (crypto random)
			var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

			var appUser = new AppUser
			{
				Name = userRegisterDto.Name,
				Surname = userRegisterDto.Surname,
				UserName = userRegisterDto.Username,
				Email = userRegisterDto.Email,
				ConfirmCode = code,
				EmailConfirmed = false
			};

			var result = await _userManager.CreateAsync(appUser, userRegisterDto.Password);

			if (result.Succeeded)
			{
				var subject = "Email Onay Kodunuz";
				var body = $"<h3>Onay Kodunuz: <b>{code}</b></h3><p>Lütfen bu kodu doğrulama ekranına giriniz.</p>";

				await _emailService.SendAsync(appUser.Email, subject, body);

				return RedirectToAction("ConfirmMail", "Register", new { email = appUser.Email });
			}

			foreach (var item in result.Errors)
				ModelState.AddModelError("", item.Description);

			return View(userRegisterDto);
		}

		[HttpGet]
		public IActionResult ConfirmMail(string email)
		{
			return View(new ConfirmMailDto { Email = email });
		}

		[HttpPost]
		public async Task<IActionResult> ConfirmMail(ConfirmMailDto confirmMailDto)
		{
			if (!ModelState.IsValid)
				return View(confirmMailDto);

			var user = await _userManager.FindByEmailAsync(confirmMailDto.Email);
			if (user == null)
			{
				ModelState.AddModelError("", "Kullanıcı bulunamadı.");
				return View(confirmMailDto);
			}

			if (user.ConfirmCode != confirmMailDto.ConfirmCode)
			{
				ModelState.AddModelError("", "Onay kodu hatalı.");
				return View(confirmMailDto);
			}

			user.EmailConfirmed = true;
			user.ConfirmCode = null;

			await _userManager.UpdateAsync(user);

			return RedirectToAction("UserLogin", "Login");
		}
	}
}
