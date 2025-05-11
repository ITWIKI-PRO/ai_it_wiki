using ai_it_wiki.Models;
using ai_it_wiki.Services.Kwork.Models;

using Microsoft.EntityFrameworkCore;

using Pomelo.EntityFrameworkCore.MySql.Internal;

namespace ai_it_wiki.Data
{
  public class ApplicationDbContext : DbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
    {
    }

    public DbSet<Proposal> Proposals { get; set; }
    public DbSet<KworkUser> KworkUsers { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<Dialog> Dialogs { get; set; }
    public DbSet<DialogMessage> DialogMessages { get; set; }
    public DbSet<DialogSettings> DialogSettings { get; set; }

    public DbSet<UserActionData> UserActionsData { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      var builder = new ConfigurationBuilder()
       .SetBasePath(Environment.CurrentDirectory)
       .AddJsonFile("appsettings.json");

      var connectionString = builder.Build().GetSection("ConnectionStrings:context").Value;
      optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 23)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null
        ));
      base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // Конфигурация сущностей, ключей, связей и других параметров
      modelBuilder.Entity<User>().HasKey(u => u.UserId);
      modelBuilder.Entity<Dialog>().HasKey(d => d.DialogId);
      modelBuilder.Entity<DialogMessage>().HasKey(m => m.MessageId);
      modelBuilder.Entity<DialogSettings>().HasKey(s => s.DialogId);
    }
  }
}
