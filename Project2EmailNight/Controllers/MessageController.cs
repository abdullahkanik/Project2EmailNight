using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2EmailNight.Context;
using Project2EmailNight.Entities;

namespace Project2EmailNight.Controllers
{
	public class MessageController : Controller
	{
		private readonly EmailContext _emailContext;
		private readonly UserManager<AppUser> _userManager;

		public MessageController(EmailContext emailContext, UserManager<AppUser> userManager)
		{
			_emailContext = emailContext;
			_userManager = userManager;
		}

		// INBOX: Bana gelenler (Çöp değil + Spam değil)
		public async Task<IActionResult> Inbox()
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null) return RedirectToAction("UserLogin", "Login");

			var messageList = await _emailContext.Messages
				.Where(x => x.ReceiverEmail == user.Email && !x.IsTrash && !x.IsSpam)
				.OrderByDescending(x => x.SendDate)
				.ToListAsync();

			return View(messageList);
		}

		// SENT: Gönderdiklerim (Çöp değil)
		public async Task<IActionResult> Sent()
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null) return RedirectToAction("UserLogin", "Login");

			var messageList = await _emailContext.Messages
				.Where(x => x.SenderEmail == user.Email && !x.IsTrash)
				.OrderByDescending(x => x.SendDate)
				.ToListAsync();

			return View(messageList);
		}

		[HttpGet]
		public IActionResult CreateMessage()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreateMessage(Message message)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Unauthorized();

			message.SenderEmail = user.Email;
			message.SendDate = DateTime.Now;

			// yeni mesaj alıcı için okunmadı olmalı
			message.IsStatus = false;

			// ✅ varsayılanlar
			message.IsStarred = false;
			message.IsTrash = false;
			message.IsSpam = false;

			_emailContext.Messages.Add(message);
			await _emailContext.SaveChangesAsync();

			return Ok();
		}

		// MESSAGE DETAIL
		public async Task<IActionResult> MessageDetail(int id)
		{
			var msg = await _emailContext.Messages.FindAsync(id);
			if (msg == null)
				return NotFound();

			// okunmadıysa okundu yap
			if (msg.IsStatus == false)
			{
				msg.IsStatus = true;
				await _emailContext.SaveChangesAsync();
			}

			return View(msg);
		}

		// 🗑️ DELETE yerine: Çöpe Taşı
		public async Task<IActionResult> DeleteMessage(int id)
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null) return RedirectToAction("UserLogin", "Login");

			var msg = await _emailContext.Messages
				.FirstOrDefaultAsync(x => x.MessageId == id && x.ReceiverEmail == user.Email);

			if (msg == null)
				return NotFound();

			msg.IsTrash = true;
			await _emailContext.SaveChangesAsync();

			return RedirectToAction("Inbox");
		}
		// ⭐ Yıldızlı
		[HttpGet]
		public async Task<IActionResult> Starred()
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null) return RedirectToAction("UserLogin", "Login");

			var messageList = await _emailContext.Messages
				.Where(x => x.ReceiverEmail == user.Email && x.IsStarred && !x.IsTrash && !x.IsSpam)
				.OrderByDescending(x => x.SendDate)
				.ToListAsync();

			return View(messageList); // Views/Message/Starred.cshtml
		}

		// 🗑️ Çöp Kutusu
		[HttpGet]
		public async Task<IActionResult> Trash()
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null) return RedirectToAction("UserLogin", "Login");

			var messageList = await _emailContext.Messages
				.Where(x => x.ReceiverEmail == user.Email && x.IsTrash)
				.OrderByDescending(x => x.SendDate)
				.ToListAsync();

			return View(messageList); // Views/Message/Trash.cshtml
		}

		// 🚫 Spam
		[HttpGet]
		public async Task<IActionResult> Spam()
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null) return RedirectToAction("UserLogin", "Login");

			var messageList = await _emailContext.Messages
				.Where(x => x.ReceiverEmail == user.Email && x.IsSpam && !x.IsTrash)
				.OrderByDescending(x => x.SendDate)
				.ToListAsync();

			return View(messageList); // Views/Message/Spam.cshtml
		}

		// ⭐ Yıldız Toggle (Inbox’taki yıldız ikonu buna vuracak)
		[HttpPost]
		public async Task<IActionResult> ToggleStar(int id)
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null) return RedirectToAction("UserLogin", "Login");

			var msg = await _emailContext.Messages
				.FirstOrDefaultAsync(x => x.MessageId == id && x.ReceiverEmail == user.Email);

			if (msg == null) return NotFound();

			msg.IsStarred = !msg.IsStarred;
			await _emailContext.SaveChangesAsync();

			return Redirect(Request.Headers["Referer"].ToString());
		}

		// 🚫 Spam’a taşı
		[HttpPost]
		public async Task<IActionResult> MoveToSpam(int id)
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null) return RedirectToAction("UserLogin", "Login");

			var msg = await _emailContext.Messages
				.FirstOrDefaultAsync(x => x.MessageId == id && x.ReceiverEmail == user.Email);

			if (msg == null) return NotFound();

			msg.IsSpam = true;
			await _emailContext.SaveChangesAsync();

			return Redirect(Request.Headers["Referer"].ToString());
		}

		// ♻️ Çöp kutusundan geri al
		[HttpPost]
		public async Task<IActionResult> RestoreFromTrash(int id)
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null) return RedirectToAction("UserLogin", "Login");

			var msg = await _emailContext.Messages
				.FirstOrDefaultAsync(x => x.MessageId == id && x.ReceiverEmail == user.Email);

			if (msg == null) return NotFound();

			msg.IsTrash = false;
			await _emailContext.SaveChangesAsync();

			return Redirect(Request.Headers["Referer"].ToString());
		}

		// ❌ Çöp kutusunda kalıcı sil (istersen)
		[HttpPost]
		public async Task<IActionResult> HardDelete(int id)
		{
			var user = await _userManager.FindByNameAsync(User.Identity.Name);
			if (user == null) return RedirectToAction("UserLogin", "Login");

			var msg = await _emailContext.Messages
				.FirstOrDefaultAsync(x => x.MessageId == id && x.ReceiverEmail == user.Email && x.IsTrash);

			if (msg == null) return NotFound();

			_emailContext.Messages.Remove(msg);
			await _emailContext.SaveChangesAsync();

			return RedirectToAction("Trash");
		}
	}
}