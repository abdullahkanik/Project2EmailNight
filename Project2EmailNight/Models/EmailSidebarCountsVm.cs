namespace Project2EmailNight.Models
{
	public class EmailSidebarCountsVm
	{
		public int InboxTotal { get; set; }
		public int InboxUnread { get; set; }
		public int SentTotal { get; set; }

		public int StarCount { get; set; }
		public int TrashCount { get; set; }
		public int DraftCount { get; set; }
		public int SpamCount { get; set; }

		public int CatEdu { get; set; }
		public int CatProje { get; set; }
		public int CatIs { get; set; }
		public int CatDiger { get; set; }
	}
}