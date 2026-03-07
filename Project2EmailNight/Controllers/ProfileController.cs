using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Project2EmailNight.Dtos;
using Project2EmailNight.Entities;

namespace Project2EmailNight.Controllers
{
	public class ProfileController : Controller
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ProfileController(UserManager<AppUser> userManager, IWebHostEnvironment webHostEnvironment)
		{
			_userManager = userManager;
			_webHostEnvironment = webHostEnvironment;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var values = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (values == null)
			{
				return RedirectToAction("UserLogin", "Login");
			}

			UserEditDto userEditDto = new UserEditDto
			{
				Name = values.Name,
				Email = values.Email,
				Surname = values.Surname,
				ImageUrl = values.ImageUrl
			};

			return View(userEditDto);
		}

		[HttpPost]
		public async Task<IActionResult> Index(UserEditDto userEditDto)
		{
			var user = await _userManager.FindByNameAsync(User.Identity!.Name!);

			if (user == null)
			{
				return RedirectToAction("UserLogin", "Login");
			}

			user.Name = userEditDto.Name;
			user.Surname = userEditDto.Surname;
			user.Email = userEditDto.Email;
			user.UserName = userEditDto.Email;

			// Şifre sadece doluysa değişsin
			if (!string.IsNullOrWhiteSpace(userEditDto.Password))
			{
				if (userEditDto.Password != userEditDto.ConfirmPassword)
				{
					TempData["ErrorMessage"] = "Şifreler eşleşmiyor.";
					userEditDto.ImageUrl = user.ImageUrl;
					return View(userEditDto);
				}

				user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userEditDto.Password);
			}

			// Resim seçildiyse kaydet
			if (userEditDto.Image != null && userEditDto.Image.Length > 0)
			{
				var extension = Path.GetExtension(userEditDto.Image.FileName);
				var imageName = Guid.NewGuid().ToString() + extension;

				var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");

				if (!Directory.Exists(folderPath))
				{
					Directory.CreateDirectory(folderPath);
				}

				var saveLocation = Path.Combine(folderPath, imageName);

				using (var stream = new FileStream(saveLocation, FileMode.Create))
				{
					await userEditDto.Image.CopyToAsync(stream);
				}

				// Veritabanına web path kaydet
				user.ImageUrl = "/images/" + imageName;
			}

			var result = await _userManager.UpdateAsync(user);

			if (result.Succeeded)
			{
				TempData["SuccessMessage"] = "Profil başarıyla güncellendi.";
				return RedirectToAction("Index", "Profile");
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error.Description);
			}

			userEditDto.ImageUrl = user.ImageUrl;
			TempData["ErrorMessage"] = "Profil güncellenirken hata oluştu.";
			return View(userEditDto);
		}
	}
}