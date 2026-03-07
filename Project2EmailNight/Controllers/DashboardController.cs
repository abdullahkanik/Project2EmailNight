using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project2EmailNight.Context;

namespace Project2EmailNight.Controllers
{
	public class DashboardController : Controller
	{
		private readonly EmailContext _context;

		public DashboardController(EmailContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var totalMessageCount = await _context.Messages.CountAsync();
			var unreadMessageCount = await _context.Messages.CountAsync(x => x.IsStatus == false && !x.IsTrash && !x.IsSpam);
			var starredCount = await _context.Messages.CountAsync(x => x.IsStarred);
			var spamCount = await _context.Messages.CountAsync(x => x.IsSpam);
			var trashCount = await _context.Messages.CountAsync(x => x.IsTrash);
			var sentMessageCount = await _context.Messages.CountAsync(x => x.SenderEmail == "abdullahkanik5@gmail.com");
			var todayInboxCount = await _context.Messages.CountAsync(x => x.SendDate.Date == DateTime.Today && !x.IsTrash && !x.IsSpam);
			var inboxCount = await _context.Messages.CountAsync(x => !x.IsTrash && !x.IsSpam);
			var thisWeekCount = await _context.Messages.CountAsync(x => x.SendDate >= DateTime.Today.AddDays(-7));

			var totalForRate = totalMessageCount == 0 ? 1 : totalMessageCount;

			ViewBag.TotalMessageCount = totalMessageCount;
			ViewBag.UnreadMessageCount = unreadMessageCount;
			ViewBag.StarredCount = starredCount;
			ViewBag.SpamCount = spamCount;
			ViewBag.TrashCount = trashCount;
			ViewBag.SentMessageCount = sentMessageCount;
			ViewBag.TodayInboxCount = todayInboxCount;
			ViewBag.InboxCount = inboxCount;
			ViewBag.ThisWeekCount = thisWeekCount;

			ViewBag.InboxRate = (int)Math.Round((double)inboxCount * 100 / totalForRate);
			ViewBag.StarRate = (int)Math.Round((double)starredCount * 100 / totalForRate);
			ViewBag.SpamRate = (int)Math.Round((double)spamCount * 100 / totalForRate);
			ViewBag.TrashRate = (int)Math.Round((double)trashCount * 100 / totalForRate);

			ViewBag.MessageGrowth = 18;
			ViewBag.DraftCount = 4;
			ViewBag.LastMailHour = DateTime.Now.ToString("HH:mm");

			ViewBag.LastMessages = await _context.Messages
				.Where(x => !x.IsTrash)
				.OrderByDescending(x => x.SendDate)
				.Take(5)
				.ToListAsync();

			return View();
		}
	}
}