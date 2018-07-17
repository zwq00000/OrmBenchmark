using OrmBenchmark.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmBenchmark.EntityFramework {
    public class EntityFrameworkExecuter : IOrmExecuter {
        OrmBenchmarkContext ctx;

        public string Name {
            get {
                return "Entity Framework";
            }
        }

        public void Init(string connectionStrong) {

            ctx = new OrmBenchmarkContext(connectionStrong);

        }

        public IPost GetItemAsObject(int id) {
            //return ctx.Posts.Where(p => p.Id == id) as IPost;
            return ctx.Posts.Find(id);

        }

        public dynamic GetItemAsDynamic(int id) {
            return ctx.Database.SqlQuery<Post>($"Select * From Posts Where Id={id}").First();
           // return null;
        }

        public IList<IPost> GetAllItemsAsObject() {
            return ctx.Posts.ToList<IPost>();
        }

        public IList<dynamic> GetAllItemsAsDynamic() {
            return ctx.Posts.ToList<dynamic>();
        }

        public void Finish() {
            ctx.Dispose();
        }
    }
}
