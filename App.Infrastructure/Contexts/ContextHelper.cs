using System.Reflection;
using App.Domain.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace App.Infrastructure.Contexts
{
    public static class ContextHelper
    {
        public static MainDBContext CreateContext<TEntity>(IDbContextTransaction? dbTransaction)
        {

            var attrEntity = typeof(TEntity).GetCustomAttribute(typeof(EntityAttribute)) as EntityAttribute;
            if (attrEntity == null || attrEntity.DBName == null)
                throw new Exception($"Not Found Entity Attribute For {typeof(TEntity).Name}");

            if (dbTransaction == null) return new MainDBContext(attrEntity.DBName);

            DbContextOptions<ShookaDbContext> options = new DbContextOptionsBuilder<ShookaDbContext>()
                .UseSqlServer(dbTransaction.GetDbTransaction().Connection)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;
            return new MainDBContext(options, attrEntity.DBName);

        }
    }

}
