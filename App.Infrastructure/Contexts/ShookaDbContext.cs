
using App.Domain.DB.Model;
using App.Domain.DB.ModelLogDB;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Contexts
{

    public partial class ShookaDbContext : DbContext
    {
        public ShookaDbContext()
        {
        }

        public ShookaDbContext(DbContextOptions<ShookaDbContext> options)
            : base(options)
        {
        }

        #region LogDB

        public virtual DbSet<AppLog> AppLogs { get; set; }
        #endregion

        #region DB


        public virtual DbSet<Bank> Banks { get; set; }


        #endregion


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region DB 
            modelBuilder.Entity<Bank>(entity =>
          {
              entity.Property(e => e.Name).HasMaxLength(50);
          });



            #endregion


            #region LogDB
            modelBuilder.Entity<AppLog>(entity =>
            {
                entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            });

            #endregion

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}
