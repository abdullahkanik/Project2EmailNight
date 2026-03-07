using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2EmailNight.Context;
using Project2EmailNight.Entities;
using Project2EmailNight.Models;

namespace Project2EmailNight.ViewComponents
{
	public class _EmailSidebarCountsViewComponent : ViewComponent
	{
		private readonly EmailContext _context;
		private readonly UserManager<AppUser> _userManager;

		public _EmailSidebarCountsViewComponent(EmailContext context, UserManager<AppUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var vm = new EmailSidebarCountsVm();

			var user = await _userManager.GetUserAsync(HttpContext.User);
			if (user == null)
				return View(vm);

			var email = user.Email;

			// ✅ Inbox (çöp değil + spam değil)
			vm.InboxTotal = await _context.Messages
				.Where(x => x.ReceiverEmail == email && !x.IsTrash && !x.IsSpam)
				.CountAsync();

			vm.InboxUnread = await _context.Messages
				.Where(x => x.ReceiverEmail == email && !x.IsTrash && !x.IsSpam && x.IsStatus == false)
				.CountAsync();

			// ✅ Sent (çöp değil)
			vm.SentTotal = await _context.Messages
				.Where(x => x.SenderEmail == email && !x.IsTrash)
				.CountAsync();

			// ✅ Yıldızlı / Çöp / Spam
			vm.StarCount = await _context.Messages
				.Where(x => x.ReceiverEmail == email && x.IsStarred && !x.IsTrash && !x.IsSpam)
				.CountAsync();

			vm.TrashCount = await _context.Messages
				.Where(x => x.ReceiverEmail == email && x.IsTrash)
				.CountAsync();

			vm.SpamCount = await _context.Messages
				.Where(x => x.ReceiverEmail == email && x.IsSpam && !x.IsTrash)
				.CountAsync();

			// ✅ Taslaklar (sende taslak mantığı yoksa 0 kalsın)
			vm.DraftCount = 0;

			// ✅ Kategoriler (CategoryId eklemediysen bunlar 0 kalır)
			// vm.CatEdu = await _context.Messages.Where(...).CountAsync();  // sonra yapacağız

			// 🔥 Senin sidebar ViewBag okuyor, bu yüzden ViewBag’e basalım:
			ViewBag.InboxTotal = vm.InboxTotal;
			ViewBag.InboxUnread = vm.InboxUnread;
			ViewBag.SentTotal = vm.SentTotal;

			ViewBag.StarCount = vm.StarCount;
			ViewBag.TrashCount = vm.TrashCount;
			ViewBag.SpamCount = vm.SpamCount;
			ViewBag.DraftCount = vm.DraftCount;

			ViewBag.CatEdu = vm.CatEdu;
			ViewBag.CatProje = vm.CatProje;
			ViewBag.CatIs = vm.CatIs;
			ViewBag.CatDiger = vm.CatDiger;

			return View(vm);
		}
	}
}