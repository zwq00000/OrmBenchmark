using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OrmBenchmark.Core;

namespace OrmBenchmark.EntityFrameworkCore
{
    public class EntityFrameworkCoreExecuter : IOrmExecuter
    {
        OrmBenchmarkContext ctx;
        
        public string Name
        {
            get
            {
                return "Entity Framework Core";
            }
        }

        public void Init(string connectionStrong) {
            var builder = new DbContextOptionsBuilder<OrmBenchmarkContext>();
            builder.UseSqlServer(connectionStrong);
            ctx = new OrmBenchmarkContext(builder.Options);
            ctx.ChangeTracker.AutoDetectChangesEnabled = false;
            ctx.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ctx.ChangeTracker.LazyLoadingEnabled = false;

        }

        public IPost GetItemAsObject(int id)
        {
            //return ctx.Posts.Where(p => p.Id == Id) as IPost;
            return ctx.Posts.FirstOrDefault(p=>p.Id ==id);//.Find(id);

        }

        public dynamic GetItemAsDynamic(int id) {
            return ctx.Posts.FromSql("select * from Posts Where Id={0}",id).First();
        }

        public IList<IPost> GetAllItemsAsObject()
        {
            return ctx.Posts.ToImmutableArray<IPost>();
        }

        public IList<dynamic> GetAllItemsAsDynamic()
        {
            return ctx.Posts.FromSql("select * from Posts").ToImmutableArray<dynamic>();
        }
        public void Finish()
        {
            ctx.Dispose();
        }
    }
}
