using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project2EmailNight.Entities;

namespace Project2EmailNight.Context
{
	public class EmailContext : IdentityDbContext<AppUser>
	{
		public EmailContext(DbContextOptions<EmailContext> options) : base(options)
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlServer("server=DESKTOP-59CMCNP\\MSSQLSERVER02;initial catalog=Project2EmailNightDb;integrated security=true;TrustServerCertificate=True");
			}
		}

		public DbSet<Message> Messages { get; set; }
	}
}
