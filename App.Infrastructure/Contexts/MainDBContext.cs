using App.Domain.DB;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Contexts
{
    public class MainDBContext : ShookaDbContext
    {
        private string? DBName { get; set; }
        public MainDBContext(string? dBName)
        {
            DBName = dBName;
        }

        public MainDBContext(DbContextOptions<ShookaDbContext> options, string? dBName)
            : base(options)
        {
            DBName = dBName;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(App.Config.ConfigHelper.GetConnectionString(DBName));
            }
        }

        public virtual DbSet<TableDTO> TableDTOs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TableDTO>().HasNoKey();

            base.OnModelCreating(modelBuilder);
        }
    }
}
